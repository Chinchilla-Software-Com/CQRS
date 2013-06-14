using System;
using Cqrs.Domain;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain
{
	[TestFixture]
	public class When_saving_stale_data
	{
		private TestInMemoryEventStore _eventStore;
		private TestAggregate _aggregate;
		private TestEventPublisher _eventPublisher;
		private Repository _rep;
		private UnitOfWork _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_eventStore = new TestInMemoryEventStore();
			_eventPublisher = new TestEventPublisher();
			var aggregateFactory = new AggregateFactory();
			_rep = new Repository(aggregateFactory, _eventStore, _eventPublisher);
			_unitOfWork = new UnitOfWork(_rep);

			_aggregate = new TestAggregate(Guid.NewGuid());
			_aggregate.DoSomething();
			_rep.Save(_aggregate);

		}

		[Test]
		public void Should_throw_concurrency_exception_from_repository()
		{
			Assert.Throws<ConcurrencyException>(() => _rep.Save(_aggregate, 0));
		}

		[Test]
		public void Should_throw_concurrency_exception_from_session()
		{
			_unitOfWork.Add(_aggregate);
			_aggregate.DoSomething();
			_rep.Save(_aggregate);
			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Commit());
		}
	}
}