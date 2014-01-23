using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Snapshots
{
	public class SnapshotRepository<TAuthenticationToken> : IRepository<TAuthenticationToken>
	{
		private ISnapshotStore SnapshotStore { get; set; }

		private ISnapshotStrategy<TAuthenticationToken> SnapshotStrategy { get; set; }

		private IRepository<TAuthenticationToken> Repository { get; set; }

		private IEventStore<TAuthenticationToken> EventStore { get; set; }

		private IAggregateFactory AggregateFactory { get; set; }

		public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy<TAuthenticationToken> snapshotStrategy, IRepository<TAuthenticationToken> repository, IEventStore<TAuthenticationToken> eventStore, IAggregateFactory aggregateFactory)
		{
			SnapshotStore = snapshotStore;
			SnapshotStrategy = snapshotStrategy;
			Repository = repository;
			EventStore = eventStore;
			AggregateFactory = aggregateFactory;
		}

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? exectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			TryMakeSnapshot(aggregate);
			Repository.Save(aggregate, exectedVersion);
		}

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();
			int snapshotVersion = TryRestoreAggregateFromSnapshot(aggregateId, aggregate);
			if (snapshotVersion == -1)
			{
				return Repository.Get<TAggregateRoot>(aggregateId);
			}
			IEnumerable<IEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TAggregateRoot>(aggregateId, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
			aggregate.LoadFromHistory(theseEvents);

			return aggregate;
		}

		private int TryRestoreAggregateFromSnapshot<TAggregateRoot>(Guid id, TAggregateRoot aggregate)
		{
			var version = -1;
			if (SnapshotStrategy.IsSnapshotable(typeof(TAggregateRoot)))
			{
				var snapshot = SnapshotStore.Get(id);
				if (snapshot != null)
				{
					aggregate.AsDynamic().Restore(snapshot);
					version = snapshot.Version;
				}
			}
			return version;
		}

		private void TryMakeSnapshot(IAggregateRoot<TAuthenticationToken> aggregate)
		{
			if (!SnapshotStrategy.ShouldMakeSnapShot(aggregate))
				return;
			var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
			snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
			SnapshotStore.Save(snapshot);
		}
	}
}