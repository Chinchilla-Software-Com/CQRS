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
	public abstract class AkkaSaga<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
		, ISaga<TAuthenticationToken>
	{
		protected ISagaUnitOfWork<TAuthenticationToken> UnitOfWork { get; set; }

		protected IAkkaSagaRepository<TAuthenticationToken> Repository { get; set; }

		protected ILogger Logger { get; set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; set; }

		private ICollection<ISagaEvent<TAuthenticationToken>> Changes { get; set; }

		public Guid Id { get; protected set; }

		public int Version { get; protected set; }

		protected ICommandPublisher<TAuthenticationToken> CommandPublisher { get; set; }

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

		public IEnumerable<ISagaEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		public virtual void MarkChangesAsCommitted()
		{
			Version = Version + Changes.Count;
			Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(new List<ISagaEvent<TAuthenticationToken>>());
		}

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

		protected virtual void ApplyChange(ISagaEvent<TAuthenticationToken> @event)
		{
			ApplyChange(@event, false);
		}

		protected virtual void ApplyChange(IEvent<TAuthenticationToken> @event)
		{
			var sagaEvent = new SagaEvent<TAuthenticationToken>(@event);
			// Set ID
			SetId(sagaEvent);
			ApplyChange(sagaEvent);
		}
		
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

		protected virtual void Apply(ISagaEvent<TAuthenticationToken> sagaEvent)
		{
			this.AsDynamic().Apply(sagaEvent.Event);
		}
	}
}