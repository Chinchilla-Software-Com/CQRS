using System;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Snapshots;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Snapshots
{
	[TestFixture]
	public class When_getting_snapshotable_aggreate
	{
		private TestSnapshotStore _snapshotStore;
		private TestSnapshotAggregate _aggregate;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestInMemoryEventStore();
			var eventPublisher = new TestEventPublisher();
			_snapshotStore = new TestSnapshotStore();
			var snapshotStrategy = new DefaultSnapshotStrategy<ISingleSignOnToken>();
			var aggregateFactory = new AggregateFactory(new TestDependencyResolver());
			var repository = new SnapshotRepository<ISingleSignOnToken>(_snapshotStore, snapshotStrategy, new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher), eventStore, aggregateFactory);
			var session = new UnitOfWork<ISingleSignOnToken>(repository);

			_aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid());
		}

		[Test]
		public void Should_ask_for_snapshot()
		{
			Assert.True(_snapshotStore.VerifyGet);
		}

		[Test]
		public void Should_run_restore_method()
		{
			Assert.True(_aggregate.Restored);
		}
	}
}
