using System;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Domain.Exceptions;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain
{
	[TestFixture]
	public class When_adding_aggregates_to_repository
	{
		private UnitOfWork<ISingleSignOnToken> _unitOfWork;

		[SetUp]
		public void SetUp()
		{
			var eventStore = new TestEventStore();
			var dependencyResolver = new TestDependencyResolver(eventStore);
			var aggregateFactory = new AggregateFactory(dependencyResolver, dependencyResolver.Resolve<ILogger>());
			var eventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher, new NullCorrelationIdHelper()));
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