using System;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain
{
	[TestFixture]
	public class When_getting_an_aggregate
	{
		private IUnitOfWork<ISingleSignOnToken> _unitOfWork;

		private TestDependencyResolver _dependencyResolver;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestEventStore();
			_dependencyResolver = new TestDependencyResolver(eventStore);
			var aggregateFactory = new AggregateFactory(_dependencyResolver, _dependencyResolver.Resolve<ILogger>());
			var testEventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, testEventPublisher, new NullCorrelationIdHelper()));
		}

		[Test]
		public void Should_get_aggregate_from_eventstore()
		{
			_dependencyResolver.UseTestEventStoreGuid = false;
			var aggregate = _unitOfWork.Get<TestAggregate>(Guid.NewGuid());
			Assert.NotNull(aggregate);
		}

		[Test]
		public void Should_apply_events()
		{
			var aggregate = _unitOfWork.Get<TestAggregate>(Guid.NewGuid());
			Assert.AreEqual(2, aggregate.DidSomethingCount);
		}

		[Test]
		public void Should_fail_if_aggregate_do_not_exist()
		{
			_dependencyResolver.UseTestEventStoreGuid = true;
			Assert.Throws<AggregateNotFoundException<TestAggregate, ISingleSignOnToken>>(() => _unitOfWork.Get<TestAggregate>(Guid.Empty));
		}

		[Test]
		public void Should_track_changes()
		{
			var agg = new TestAggregate(Guid.NewGuid());
			_unitOfWork.Add(agg);
			var aggregate = _unitOfWork.Get<TestAggregate>(agg.Id);
			Assert.AreEqual(agg,aggregate);
		}

		[Test]
		public void Should_get_from_session_if_tracked()
		{
			var id = Guid.NewGuid();
			_dependencyResolver.NewAggregateGuid = id;
			var aggregate = _unitOfWork.Get<TestAggregate>(id);
			var aggregate2 = _unitOfWork.Get<TestAggregate>(id);

			Assert.AreEqual(aggregate, aggregate2);
		}

		[Test]
		public void Should_throw_concurrency_exception_if_tracked()
		{
			var id = Guid.NewGuid();
			_unitOfWork.Get<TestAggregate>(id);

			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Get<TestAggregate>(id, 100));
		}

		[Test]
		public void Should_get_correct_version()
		{
			var id = Guid.NewGuid();
			var aggregate = _unitOfWork.Get<TestAggregate>(id);

			Assert.AreEqual(4, aggregate.Version);
		}

		[Test]
		public void Should_throw_concurrency_exception()
		{
			var id = Guid.NewGuid();
			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Get<TestAggregate>(id, 1));
		}
	}
}