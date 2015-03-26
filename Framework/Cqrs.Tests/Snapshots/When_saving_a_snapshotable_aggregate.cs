using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Snapshots;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Snapshots
{
	[TestFixture]
	public class When_saving_a_snapshotable_aggregate
	{
		private TestSnapshotStore _snapshotStore;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestInMemoryEventStore();
			var eventPublisher = new TestEventPublisher();
			_snapshotStore = new TestSnapshotStore();
			var snapshotStrategy = new DefaultSnapshotStrategy<ISingleSignOnToken>();
			var aggregateFactory = new AggregateFactory(null);
			var repository = new SnapshotRepository<ISingleSignOnToken>(_snapshotStore, snapshotStrategy, new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher), eventStore, aggregateFactory);
			var session = new UnitOfWork<ISingleSignOnToken>(repository);
			var aggregate = new TestSnapshotAggregate();
			for (int i = 0; i < 30; i++)
			{
				aggregate.DoSomething();
			}
			session.Add(aggregate);
			session.Commit();
		}

		[Test]
		public void Should_save_snapshot()
		{
			Assert.True(_snapshotStore.VerifySave);
		}

		[Test]
		public void Should_save_last_version_number()
		{
			Assert.AreEqual(30, _snapshotStore.SavedVersion);
		}
	}
}
