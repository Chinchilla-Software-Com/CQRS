using System;
using Cqrs.Domain;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Repositories.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain
{
	[TestFixture]
	public class When_getting_events_out_of_order
	{
		private IUnitOfWork<ISingleSignOnToken> _unitOfWork;

		[SetUp]
		public void Setup()
		{
			var aggregateFactory = new AggregateFactory();
			var eventStore = new TestEventStoreWithBugs();
			var testEventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, testEventPublisher));
		}

		[Test]
		public void Should_throw_concurrency_exception()
		{
			var id = Guid.NewGuid();
			Assert.Throws<EventsOutOfOrderException>(() => _unitOfWork.Get<TestAggregate>(id, 3));
		}
	}
}