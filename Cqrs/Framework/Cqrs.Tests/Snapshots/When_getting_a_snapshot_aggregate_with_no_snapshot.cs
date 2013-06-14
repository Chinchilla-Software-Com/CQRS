using System;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Snapshots;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Snapshots
{
	[TestFixture]
	public class When_getting_a_snapshot_aggregate_with_no_snapshot
	{
		private TestSnapshotAggregate _aggregate;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestEventStore();
			var eventPublisher = new TestEventPublisher();
			var snapshotStore = new NullSnapshotStore();
			var snapshotStrategy = new DefaultSnapshotStrategy();
			var aggregateFactory = new AggregateFactory();
			var repository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(aggregateFactory, eventStore, eventPublisher), eventStore, aggregateFactory);
			var session = new UnitOfWork(repository);
			_aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid());
		}

		private class NullSnapshotStore : ISnapshotStore
		{
			public Snapshot Get(Guid id)
			{
				return null;
			}

			public void Save(Snapshot snapshot)
			{
			}
		}

		[Test]
		public void Should_load_events()
		{
			Assert.True(_aggregate.Loaded);
		}

		[Test]
		public void Should_not_load_snapshot()
		{
			Assert.False(_aggregate.Restored);
		}
	}
}