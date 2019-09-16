using System;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Snapshots;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Snapshots
{
	[TestFixture]
	public class When_getting_a_snapshot_aggregate_with_no_snapshot
	{
		private TestSnapshotAggregate _aggregate;

		private TestDependencyResolver _dependencyResolver;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestSnapshotEventStore();
			var eventPublisher = new TestEventPublisher();
			var snapshotStore = new NullSnapshotStore();
			var snapshotStrategy = new DefaultSnapshotStrategy<ISingleSignOnToken>();
			_dependencyResolver = new TestDependencyResolver(null);
			var aggregateFactory = new AggregateFactory(_dependencyResolver, _dependencyResolver.Resolve<ILogger>());
			var repository = new SnapshotRepository<ISingleSignOnToken>(snapshotStore, snapshotStrategy, new AggregateRepository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher, new NullCorrelationIdHelper(), new ConfigurationManager()), eventStore, aggregateFactory);
			var session = new UnitOfWork<ISingleSignOnToken>(repository);
			Guid id = Guid.NewGuid();
			_dependencyResolver.NewAggregateGuid = id;
			_aggregate = session.Get<TestSnapshotAggregate>(id);
		}

		private class NullSnapshotStore : ISnapshotStore
		{
			/// <summary>
			/// Returns null.
			/// </summary>
			public Snapshot Get<TAggregateRoot>(Guid id)
			{
				return null;
			}

			/// <summary>
			/// Get the latest <see cref="Snapshot"/> from storage.
			/// </summary>
			/// <param name="aggregateRootType">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to find a snapshot for.</param>
			/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get the most recent <see cref="Snapshot"/> of.</param>
			/// <returns>The most recent <see cref="Snapshot"/> of</returns>
			public Snapshot Get(Type aggregateRootType, Guid id)
			{
				return null;
			}

			/// <summary>
			/// Does absolutely nothing.
			/// </summary>
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