using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Authentication;
using Cqrs.Snapshots;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Snapshots
{
	[TestFixture]
	public class When_saving_a_snapshotable_aggregate_for_each_change
	{
		private TestInMemorySnapshotStore _snapshotStore;
		private IUnitOfWork<ISingleSignOnToken> _unitOfWork;
		private TestSnapshotAggregate _aggregate;

		[SetUp]
		public void Setup()
		{
			IEventStore<ISingleSignOnToken> eventStore = new TestInMemoryEventStore();
			var eventPublisher = new TestEventPublisher();
			_snapshotStore = new TestInMemorySnapshotStore();
			var snapshotStrategy = new DefaultSnapshotStrategy<ISingleSignOnToken>();
			var aggregateFactory = new AggregateFactory(null);
			var repository = new SnapshotRepository<ISingleSignOnToken>(_snapshotStore, snapshotStrategy, new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher), eventStore, aggregateFactory);
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(repository);
			_aggregate = new TestSnapshotAggregate();

			for (var i = 0; i < 20; i++)
			{
				_unitOfWork.Add(_aggregate);
				_aggregate.DoSomething();
				_unitOfWork.Commit();
			}

		}

		[Test]
		public void Should_snapshot_15th_change()
		{
			Assert.AreEqual(15, _snapshotStore.SavedVersion);
		}

		[Test]
		public void Should_not_snapshot_first_event()
		{
			Assert.False(_snapshotStore.FirstSaved);
		}

		[Test]
		public void Should_get_aggregate_back_correct()
		{
			Assert.AreEqual(20, _unitOfWork.Get<TestSnapshotAggregate>(_aggregate.Id).Number);
		}
	}
}