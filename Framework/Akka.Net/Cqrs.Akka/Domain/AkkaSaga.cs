#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// A <see cref="ISaga{TAuthenticationToken}"/> that is safe to use within Akka.NET
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public abstract class AkkaSaga<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
		, ISaga<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the <see cref="ISagaUnitOfWork{TAuthenticationToken}"/>.
		/// </summary>
		protected ISagaUnitOfWork<TAuthenticationToken> UnitOfWork { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="IAkkaSagaRepository{TAuthenticationToken}"/>.
		/// </summary>
		protected IAkkaSagaRepository<TAuthenticationToken> Repository { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; set; }

		private ICollection<ISagaEvent<TAuthenticationToken>> Changes { get; set; }

		/// <summary>
		/// The identifier of the <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		public Guid Id { get; protected set; }

		/// <summary>
		/// The current version of this <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		public int Version { get; protected set; }

		/// <summary>
		/// Gets or sets the <see cref="ICommandPublisher{TAuthenticationToken}"/>.
		/// </summary>
		protected ICommandPublisher<TAuthenticationToken> CommandPublisher { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaSaga{TAuthenticationToken}"/>
		/// </summary>
		protected AkkaSaga(ISagaUnitOfWork<TAuthenticationToken> unitOfWork, ILogger logger, IAkkaSagaRepository<TAuthenticationToken> repository, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICommandPublisher<TAuthenticationToken> commandPublisher)
		{
			UnitOfWork = unitOfWork;
			Logger = logger;
			Repository = repository;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CommandPublisher = commandPublisher;
			Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(new List<ISagaEvent<TAuthenticationToken>>());
		}

		#region Overrides of ActorBase

		/// <summary>
		/// User overridable callback.
		///                 <p/>
		///                 Is called when an Actor is started.
		///                 Actors are automatically started asynchronously when created.
		///                 Empty default implementation.
		/// </summary>
		protected override void PreStart()
		{
			base.PreStart();
			Repository.LoadSagaHistory(this, throwExceptionOnNoEvents: false);
		}

		#endregion

		/// <summary>
		/// Executes the provided <paramref name="action"/> passing it the provided <paramref name="event"/>,
		/// then calls <see cref="AggregateRepository{TAuthenticationToken}.PublishEvent"/>
		/// </summary>
		protected virtual void Execute<TEvent>(Action<TEvent> action, TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			UnitOfWork.Add(this);
			try
			{
				AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);
				CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
				action(@event);

				UnitOfWork.Commit();

				Sender.Tell(true, Self);
			}
			catch(Exception exception)
			{
				Logger.LogError("Executing an Akka.net request failed.", exception: exception, metaData: new Dictionary<string, object> { { "Type", GetType() }, { "Event", @event } });
				Sender.Tell(false, Self);
				throw;
			}
		}

		/// <summary>
		/// Get all applied changes that haven't yet been committed.
		/// </summary>
		public IEnumerable<ISagaEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		/// <summary>
		/// Mark all applied changes as committed, increment <see cref="Version"/> and flush the internal collection of changes.
		/// </summary>
		public virtual void MarkChangesAsCommitted()
		{
			Version = Version + Changes.Count;
			Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(new List<ISagaEvent<TAuthenticationToken>>());
		}

		/// <summary>
		/// Apply all the <see cref="IEvent{TAuthenticationToken}">events</see> in <paramref name="history"/>
		/// using event replay to this instance.
		/// </summary>
		public virtual void LoadFromHistory(IEnumerable<ISagaEvent<TAuthenticationToken>> history)
		{
			Type sagaType = GetType();
			foreach (ISagaEvent<TAuthenticationToken> @event in history.OrderBy(e => e.Version))
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.Id, sagaType, Version + 1, @event.Version);
				ApplyChange(@event, true);
			}
		}

		/// <summary>
		/// Call the "Apply" method with a signature matching the provided <paramref name="event"/> without using event replay to this instance.
		/// </summary>
		/// <remarks>
		/// This means a method named "Apply", with return type void and one parameter must exist to be applied.
		/// If no method exists, nothing is applied
		/// The parameter type must match exactly the <see cref="Type"/> of the provided <paramref name="event"/>.
		/// </remarks>
		protected virtual void ApplyChange(ISagaEvent<TAuthenticationToken> @event)
		{
			ApplyChange(@event, false);
		}

		/// <summary>
		/// Calls <see cref="SetId"/>, then <see cref="ApplyChange(Cqrs.Events.ISagaEvent{TAuthenticationToken})"/>.
		/// </summary>
		protected virtual void ApplyChange(IEvent<TAuthenticationToken> @event)
		{
			var sagaEvent = new SagaEvent<TAuthenticationToken>(@event);
			// Set ID
			SetId(sagaEvent);
			ApplyChange(sagaEvent);
		}

		/// <summary>
		/// Sets the <see cref="IEvent{TAuthenticationToken}.Id"/> from <see cref="ISagaEvent{TAuthenticationToken}.Event"/> back onto <paramref name="sagaEvent"/>.
		/// </summary>
		protected virtual void SetId(ISagaEvent<TAuthenticationToken> sagaEvent)
		{
			sagaEvent.Id = sagaEvent.Event.Id;
		}

		private void ApplyChange(ISagaEvent<TAuthenticationToken> @event, bool isEventReplay)
		{
			this.AsDynamic().Apply(@event);
			if (!isEventReplay)
			{
				Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(new[] { @event }.Concat(Changes).ToList());
			}
			else
			{
				Id = @event.Id;
				Version++;
			}
		}

		/// <summary>
		/// Dynamically calls the "Apply" method, passing it the <see cref="ISagaEvent{TAuthenticationToken}.Event"/> of the provided <paramref name="sagaEvent"/>.
		/// </summary>
		protected virtual void Apply(ISagaEvent<TAuthenticationToken> sagaEvent)
		{
			this.AsDynamic().Apply(sagaEvent.Event);
		}
	}
}