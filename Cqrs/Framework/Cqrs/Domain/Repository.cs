using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class Repository<TPermissionToken> : IRepository<TPermissionToken>
	{
		private IEventStore<TPermissionToken> EventStore { get; set; }

		private IEventPublisher<TPermissionToken> Publisher { get; set; }

		private IAggregateFactory AggregateFactory { get; set; }

		public Repository(IAggregateFactory aggregateFactory, IEventStore<TPermissionToken> eventStore, IEventPublisher<TPermissionToken> publisher)
		{
			EventStore = eventStore;
			Publisher = publisher;
			AggregateFactory = aggregateFactory;
		}

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TPermissionToken>
		{
			IList<IEvent<TPermissionToken>> uncommittedChanges = aggregate.GetUncommittedChanges().ToList();
			if (!uncommittedChanges.Any())
				return;

			if (expectedVersion != null)
			{
				IEnumerable<IEvent<TPermissionToken>> eventStoreResults = EventStore.Get(aggregate.Id, expectedVersion.Value);
				if (eventStoreResults.Any())
					throw new ConcurrencyException(aggregate.Id);
			}

			var eventsToPublish = new List<IEvent<TPermissionToken>>();

			int i = 0;
			foreach (IEvent<TPermissionToken> @event in uncommittedChanges)
			{
				if (@event.Id == Guid.Empty) 
					@event.Id = aggregate.Id;
				if (@event.Id == Guid.Empty)
					throw new AggregateOrEventMissingIdException(aggregate.GetType(), @event.GetType());

				i++;

				@event.Version = aggregate.Version + i;
				@event.TimeStamp = DateTimeOffset.UtcNow;
				EventStore.Save(@event);
				eventsToPublish.Add(@event);
			}

			aggregate.MarkChangesAsCommitted();
			foreach (IEvent<TPermissionToken> @event in eventsToPublish)
				Publisher.Publish(@event);
		}

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TPermissionToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionToken>
		{
			return LoadAggregate<TAggregateRoot>(aggregateId, events);
		}

		private TAggregateRoot LoadAggregate<TAggregateRoot>(Guid id, IList<IEvent<TPermissionToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionToken>
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();

			IList<IEvent<TPermissionToken>> theseEvents = events ?? EventStore.Get(id, -1).ToList();
			if (!theseEvents.Any())
				throw new AggregateNotFoundException(id);

			aggregate.LoadFromHistory(theseEvents);
			return aggregate;
		}
	}
}