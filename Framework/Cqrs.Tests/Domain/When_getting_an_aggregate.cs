using System;
using Cqrs.Domain;
using Cqrs.Domain.Exception;
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

		[SetUp]
		public void Setup()
		{
			var aggregateFactory = new AggregateFactory();
			var eventStore = new TestEventStore();
			var testEventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, testEventPublisher));
		}

		[Test]
		public void Should_get_aggregate_from_eventstore()
		{
			var aggregate = _unitOfWork.Get<TestAggregate>(Guid.NewGuid());
			Assert.NotNull(aggregate);
		}

		[Test]
		public void Should_apply_events()
		{
			var aggregate = _unitOfWork.Get<TestAggregate>(Guid.NewGuid());
			Assert.AreEqual(2,aggregate.DidSomethingCount);
		}

		[Test]
		public void Should_fail_if_aggregate_do_not_exist()
		{
			Assert.Throws<AggregateNotFoundException>(() => _unitOfWork.Get<TestAggregate>(Guid.Empty));
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

			Assert.AreEqual(3,aggregate.Version);
		}

		[Test]
		public void Should_throw_concurrency_exception()
		{
			var id = Guid.NewGuid();
			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Get<TestAggregate>(id, 1));
		}
	}
}