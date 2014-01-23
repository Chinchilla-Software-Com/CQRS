using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class Repository<TAuthenticationToken> : IRepository<TAuthenticationToken>
	{
		private IEventStore<TAuthenticationToken> EventStore { get; set; }

		private IEventPublisher<TAuthenticationToken> Publisher { get; set; }

		private IAggregateFactory AggregateFactory { get; set; }

		public Repository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher)
		{
			EventStore = eventStore;
			Publisher = publisher;
			AggregateFactory = aggregateFactory;
		}

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			IList<IEvent<TAuthenticationToken>> uncommittedChanges = aggregate.GetUncommittedChanges().ToList();
			if (!uncommittedChanges.Any())
				return;

			if (expectedVersion != null)
			{
				IEnumerable<IEvent<TAuthenticationToken>> eventStoreResults = EventStore.Get<TAggregateRoot>(aggregate.Id, expectedVersion.Value);
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
				EventStore.Save<TAggregateRoot>(@event);
				eventsToPublish.Add(@event);
			}

			aggregate.MarkChangesAsCommitted();
			foreach (IEvent<TAuthenticationToken> @event in eventsToPublish)
				Publisher.Publish(@event);
		}

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			return LoadAggregate<TAggregateRoot>(aggregateId, events);
		}

		private TAggregateRoot LoadAggregate<TAggregateRoot>(Guid id, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();

			IList<IEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TAggregateRoot>(id, -1).ToList();
			if (!theseEvents.Any())
				throw new AggregateNotFoundException(id);

			aggregate.LoadFromHistory(theseEvents);
			return aggregate;
		}
	}
}