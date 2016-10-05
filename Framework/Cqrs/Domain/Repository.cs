#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
	public class Repository<TAuthenticationToken> : IRepository<TAuthenticationToken>
	{
		protected IEventStore<TAuthenticationToken> EventStore { get; private set; }

		protected IEventPublisher<TAuthenticationToken> Publisher { get; private set; }

		protected IAggregateFactory AggregateFactory { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		public Repository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper)
		{
			EventStore = eventStore;
			Publisher = publisher;
			CorrelationIdHelper = correlationIdHelper;
			AggregateFactory = aggregateFactory;
		}

		public virtual void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			IList<IEvent<TAuthenticationToken>> uncommittedChanges = aggregate.GetUncommittedChanges().ToList();
			if (!uncommittedChanges.Any())
				return;

			if (expectedVersion != null)
			{
				IEnumerable<IEvent<TAuthenticationToken>> eventStoreResults = EventStore.Get(aggregate.GetType(), aggregate.Id, false, expectedVersion.Value);
				if (eventStoreResults.Any())
					throw new ConcurrencyException(aggregate.Id);
			}

			var eventsToPublish = new List<IEvent<TAuthenticationToken>>();

			int i = 0;
			foreach (IEvent<TAuthenticationToken> @event in uncommittedChanges)
			{
				if (@event.Id == Guid.Empty) 
					@event.Id = aggregate.Id;
				if (@event.Id == Guid.Empty)
					throw new AggregateOrEventMissingIdException(aggregate.GetType(), @event.GetType());

				i++;

				@event.Version = aggregate.Version + i;
				@event.TimeStamp = DateTimeOffset.UtcNow;
				@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();
				EventStore.Save(aggregate.GetType(), @event);
				eventsToPublish.Add(@event);
			}

			aggregate.MarkChangesAsCommitted();
			foreach (IEvent<TAuthenticationToken> @event in eventsToPublish)
				Publisher.Publish(@event);
		}

		public virtual TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			return LoadAggregate<TAggregateRoot>(aggregateId, events);
		}

		protected virtual TAggregateRoot LoadAggregate<TAggregateRoot>(Guid id, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>(id);

			IList<IEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TAggregateRoot>(id).ToList();
			if (!theseEvents.Any())
				throw new AggregateNotFoundException<TAggregateRoot, TAuthenticationToken>(id);

			var duplicatedEvents =
				theseEvents.GroupBy(x => x.Version)
					.Select(x => new {Version = x.Key, Total = x.Count()})
					.FirstOrDefault(x => x.Total > 1);
			if (duplicatedEvents != null)
				throw new DuplicateEventException<TAggregateRoot, TAuthenticationToken>(id, duplicatedEvents.Version);

			aggregate.LoadFromHistory(theseEvents);
			return aggregate;
		}
	}
}