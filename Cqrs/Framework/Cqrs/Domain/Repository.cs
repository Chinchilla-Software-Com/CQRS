using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class Repository : IRepository
	{
		private IEventStore EventStore { get; set; }
		private IEventPublisher Publisher { get; set; }
		private IAggregateFactory AggregateFactory { get; set; }

		public Repository(IAggregateFactory aggregateFactory, IEventStore eventStore, IEventPublisher publisher)
		{
			EventStore = eventStore;
			Publisher = publisher;
			AggregateFactory = aggregateFactory;
		}

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot
		{
			if (expectedVersion != null)
			{
				IEnumerable<IEvent> eventStoreResults = EventStore.Get(aggregate.Id, expectedVersion.Value);
				if (eventStoreResults.Any())
					throw new ConcurrencyException(aggregate.Id);
			}
			int i = 0;
			foreach (IEvent @event in aggregate.GetUncommittedChanges())
			{
				if (@event.Id == Guid.Empty) 
					@event.Id = aggregate.Id;
				if (@event.Id == Guid.Empty)
					throw new AggregateOrEventMissingIdException(aggregate.GetType(), @event.GetType());
				i++;
				@event.Version = aggregate.Version + i;
				@event.TimeStamp = DateTimeOffset.UtcNow;
				EventStore.Save(@event);
				Publisher.Publish(@event);
			}
			aggregate.MarkChangesAsCommitted();
		}

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent> events = null)
			where TAggregateRoot : IAggregateRoot
		{
			return LoadAggregate<TAggregateRoot>(aggregateId, events);
		}

		private TAggregateRoot LoadAggregate<TAggregateRoot>(Guid id, IList<IEvent> events = null)
			where TAggregateRoot : IAggregateRoot
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();

			IList<IEvent> theseEvents = events ?? EventStore.Get(id, -1).ToList();
			if (!theseEvents.Any())
				throw new AggregateNotFoundException(id);

			aggregate.LoadFromHistory(theseEvents);
			return aggregate;
		}
	}
}