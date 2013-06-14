using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Snapshots
{
	public class SnapshotRepository : IRepository
	{
		private ISnapshotStore SnapshotStore { get; set; }
		private ISnapshotStrategy SnapshotStrategy { get; set; }
		private IRepository Repository { get; set; }
		private IEventStore EventStore { get; set; }
		private IAggregateFactory AggregateFactory { get; set; }

		public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IRepository repository, IEventStore eventStore, IAggregateFactory aggregateFactory)
		{
			SnapshotStore = snapshotStore;
			SnapshotStrategy = snapshotStrategy;
			Repository = repository;
			EventStore = eventStore;
			AggregateFactory = aggregateFactory;
		}

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? exectedVersion = null)
			where TAggregateRoot : IAggregateRoot
		{
			TryMakeSnapshot(aggregate);
			Repository.Save(aggregate, exectedVersion);
		}

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId)
			where TAggregateRoot : IAggregateRoot
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();
			int snapshotVersion = TryRestoreAggregateFromSnapshot(aggregateId, aggregate);
			if(snapshotVersion == -1)
			{
				return Repository.Get<TAggregateRoot>(aggregateId);
			}
			IEnumerable<IEvent> events = EventStore.Get(aggregateId, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
			aggregate.LoadFromHistory(events);

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

		private void TryMakeSnapshot(IAggregateRoot aggregate)
		{
			if (!SnapshotStrategy.ShouldMakeSnapShot(aggregate))
				return;
			var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
			snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
			SnapshotStore.Save(snapshot);
		}
	}
}