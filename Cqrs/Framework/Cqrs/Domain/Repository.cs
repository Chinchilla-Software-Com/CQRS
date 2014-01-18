using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class Repository<TPermissionScope> : IRepository<TPermissionScope>
	{
		private IEventStore<TPermissionScope> EventStore { get; set; }

		private IEventPublisher<TPermissionScope> Publisher { get; set; }

		private IAggregateFactory AggregateFactory { get; set; }

		public Repository(IAggregateFactory aggregateFactory, IEventStore<TPermissionScope> eventStore, IEventPublisher<TPermissionScope> publisher)
		{
			EventStore = eventStore;
			Publisher = publisher;
			AggregateFactory = aggregateFactory;
		}

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>
		{
			IList<IEvent<TPermissionScope>> uncommittedChanges = aggregate.GetUncommittedChanges().ToList();
			if (!uncommittedChanges.Any())
				return;

			if (expectedVersion != null)
			{
				IEnumerable<IEvent<TPermissionScope>> eventStoreResults = EventStore.Get(aggregate.Id, expectedVersion.Value);
				if (eventStoreResults.Any())
					throw new ConcurrencyException(aggregate.Id);
			}

			var eventsToPublish = new List<IEvent<TPermissionScope>>();

			int i = 0;
			foreach (IEvent<TPermissionScope> @event in uncommittedChanges)
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
			foreach (IEvent<TPermissionScope> @event in eventsToPublish)
				Publisher.Publish(@event);
		}

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TPermissionScope>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>
		{
			return LoadAggregate<TAggregateRoot>(aggregateId, events);
		}

		private TAggregateRoot LoadAggregate<TAggregateRoot>(Guid id, IList<IEvent<TPermissionScope>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();

			IList<IEvent<TPermissionScope>> theseEvents = events ?? EventStore.Get(id, -1).ToList();
			if (!theseEvents.Any())
				throw new AggregateNotFoundException(id);

			aggregate.LoadFromHistory(theseEvents);
			return aggregate;
		}
	}
}