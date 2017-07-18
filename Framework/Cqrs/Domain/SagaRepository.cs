#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Domain.Exceptions;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class SagaRepository<TAuthenticationToken> : ISagaRepository<TAuthenticationToken>
	{
		protected IEventStore<TAuthenticationToken> EventStore { get; private set; }

		protected IEventPublisher<TAuthenticationToken> Publisher { get; private set; }

		protected IAggregateFactory SagaFactory { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		public SagaRepository(IAggregateFactory sagaFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper)
		{
			EventStore = eventStore;
			Publisher = publisher;
			CorrelationIdHelper = correlationIdHelper;
			SagaFactory = sagaFactory;
		}

		public virtual void Save<TSaga>(TSaga saga, int? expectedVersion = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			IList<ISagaEvent<TAuthenticationToken>> uncommittedChanges = saga.GetUncommittedChanges().ToList();
			if (!uncommittedChanges.Any())
				return;

			if (expectedVersion != null)
			{
				IEnumerable<IEvent<TAuthenticationToken>> eventStoreResults = EventStore.Get(saga.GetType(), saga.Id, false, expectedVersion.Value);
				if (eventStoreResults.Any())
					throw new ConcurrencyException(saga.Id);
			}

			var eventsToPublish = new List<ISagaEvent<TAuthenticationToken>>();

			int i = 0;
			int version = saga.Version;
			foreach (ISagaEvent<TAuthenticationToken> @event in uncommittedChanges)
			{
				if (@event.Id == Guid.Empty)
					@event.Id = saga.Id;
				if (@event.Id == Guid.Empty)
					throw new AggregateOrEventMissingIdException(saga.GetType(), @event.GetType());

				i++;
				version++;

				@event.Version = version;
				@event.TimeStamp = DateTimeOffset.UtcNow;
				@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();
				EventStore.Save(saga.GetType(), @event);
				eventsToPublish.Add(@event);
			}

			saga.MarkChangesAsCommitted();
			foreach (ISagaEvent<TAuthenticationToken> @event in eventsToPublish)
				PublishEvent(@event);
		}

		protected virtual void PublishEvent(ISagaEvent<TAuthenticationToken> @event)
		{
			Publisher.Publish(@event);
		}

		public virtual TSaga Get<TSaga>(Guid sagaId, IList<ISagaEvent<TAuthenticationToken>> events = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			return LoadSaga<TSaga>(sagaId, events);
		}

		protected virtual TSaga CreateSaga<TSaga>(Guid id)
			where TSaga : ISaga<TAuthenticationToken>
		{
			var saga = SagaFactory.Create<TSaga>(id);

			return saga;
		}

		protected virtual TSaga LoadSaga<TSaga>(Guid id, IList<ISagaEvent<TAuthenticationToken>> events = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			var saga = SagaFactory.Create<TSaga>(id);

			LoadSagaHistory(saga, events);
			return saga;
		}

		public virtual void LoadSagaHistory<TSaga>(TSaga saga, IList<ISagaEvent<TAuthenticationToken>> events = null, bool throwExceptionOnNoEvents = true)
			where TSaga : ISaga<TAuthenticationToken>
		{
			IList<ISagaEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TSaga>(saga.Id).Cast<ISagaEvent<TAuthenticationToken>>().ToList();
			if (!theseEvents.Any())
			{
				if (throwExceptionOnNoEvents)
					throw new SagaNotFoundException<TSaga, TAuthenticationToken>(saga.Id);
				return;
			}

			var duplicatedEvents =
				theseEvents.GroupBy(x => x.Version)
					.Select(x => new { Version = x.Key, Total = x.Count() })
					.FirstOrDefault(x => x.Total > 1);
			if (duplicatedEvents != null)
				throw new DuplicateSagaEventException<TSaga, TAuthenticationToken>(saga.Id, duplicatedEvents.Version);

			saga.LoadFromHistory(theseEvents);
		}
	}
}