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
using Cqrs.Akka.Snapshots;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// An <see cref="IAggregateRoot{TAuthenticationToken}"/> that is safe to use within Akka.NET
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public abstract class AkkaAggregateRoot<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
		, IAggregateRoot<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the <see cref="IUnitOfWork{TAuthenticationToken}"/>.
		/// </summary>
		protected IUnitOfWork<TAuthenticationToken> UnitOfWork { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="IAkkaAggregateRepository{TAuthenticationToken}"/>.
		/// </summary>
		protected IAkkaAggregateRepository<TAuthenticationToken> Repository { get; set; }

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

		private ICollection<IEvent<TAuthenticationToken>> Changes { get; set; }

		/// <summary>
		/// The identifier of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		public Guid Id { get; protected set; }

		/// <summary>
		/// The current version of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		public int Version { get; protected set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaAggregateRoot{TAuthenticationToken}"/>
		/// </summary>
		protected AkkaAggregateRoot(IUnitOfWork<TAuthenticationToken> unitOfWork, ILogger logger, IAkkaAggregateRepository<TAuthenticationToken> repository, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			UnitOfWork = unitOfWork;
			Logger = logger;
			Repository = repository;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
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
			Repository.LoadAggregateHistory(this, throwExceptionOnNoEvents: false);
		}

		#endregion

		/// <summary>
		/// Executes the provided <paramref name="action"/> passing it the provided <paramref name="command"/>,
		/// then calls <see cref="AggregateRepository{TAuthenticationToken}.PublishEvent"/>
		/// </summary>
		protected virtual void Execute<TCommand>(Action<TCommand> action, TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type baseType = GetType().BaseType;
			UnitOfWork.Add(this, baseType != null && baseType.Name == typeof(AkkaSnapshotAggregateRoot<,>).Name);
			try
			{
				AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);
				CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
				action(command);

				UnitOfWork.Commit();

				Sender.Tell(true, Self);
			}
			catch(Exception exception)
			{
				Logger.LogError("Executing an Akka.net request failed.", exception: exception, metaData: new Dictionary<string, object> { { "Type", GetType() }, { "Command", command } });
				Sender.Tell(false, Self);
				throw;
			}
		}

		/// <summary>
		/// Get all applied changes that haven't yet been committed.
		/// </summary>
		public IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		/// <summary>
		/// Mark all applied changes as committed, increment <see cref="Version"/> and flush the internal collection of changes.
		/// </summary>
		public virtual void MarkChangesAsCommitted()
		{
			Version = Version + Changes.Count;
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		/// <summary>
		/// Apply all the <see cref="IEvent{TAuthenticationToken}">events</see> in <paramref name="history"/>
		/// using event replay to this instance.
		/// </summary>
		public virtual void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			Type aggregateType = GetType();
			foreach (IEvent<TAuthenticationToken> @event in history.OrderBy(e =>e.Version))
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.GetIdentity(), aggregateType, Version + 1, @event.Version);
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
		protected virtual void ApplyChange(IEvent<TAuthenticationToken> @event)
		{
			ApplyChange(@event, false);
		}

		private void ApplyChange(IEvent<TAuthenticationToken> @event, bool isEventReplay)
		{
			this.AsDynamic().Apply(@event);
			if (!isEventReplay)
			{
				Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new []{@event}.Concat(Changes).ToList());
			}
			else
			{
				Id = @event.GetIdentity();
				Version++;
			}
		}
	}
}