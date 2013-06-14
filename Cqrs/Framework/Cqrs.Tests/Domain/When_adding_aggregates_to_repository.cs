using System;
using Cqrs.Domain;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain
{
	[TestFixture]
	public class When_adding_aggregates_to_repository
	{
		private UnitOfWork _unitOfWork;

		[SetUp]
		public void SetUp()
		{
			var aggregateFactory = new AggregateFactory();
			var eventStore = new TestInMemoryEventStore();
			var eventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork(new Repository(aggregateFactory, eventStore, eventPublisher));
		}

		[Test]
		public void Should_throw_if_different_object_with_tracked_guid_is_added()
		{
			var aggregate = new TestAggregate(Guid.NewGuid());
			var aggregate2 = new TestAggregate(aggregate.Id);
			_unitOfWork.Add(aggregate);
			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Add(aggregate2));
		}

		[Test]
		public void Should_not_throw_if_object_already_tracked()
		{
			var aggregate = new TestAggregate(Guid.NewGuid());
			_unitOfWork.Add(aggregate);
			_unitOfWork.Add(aggregate);
		}
	}
}