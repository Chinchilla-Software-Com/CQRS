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
	public class When_saving_stale_data
	{
		private TestInMemoryEventStore _eventStore;
		private TestAggregate _aggregate;
		private TestEventPublisher _eventPublisher;
		private Repository<ISingleSignOnToken> _rep;
		private UnitOfWork<ISingleSignOnToken> _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_eventStore = new TestInMemoryEventStore();
			_eventPublisher = new TestEventPublisher();
			var aggregateFactory = new AggregateFactory(new TestDependencyResolver());
			_rep = new Repository<ISingleSignOnToken>(aggregateFactory, _eventStore, _eventPublisher, new NullCorrelationIdHelper());
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(_rep);

			_aggregate = new TestAggregate(Guid.NewGuid());
			_aggregate.DoSomething();
			_rep.Save(_aggregate);

		}

		[Test]
		public void Should_throw_concurrency_exception_from_repository()
		{
			_aggregate.DoSomething();
			Assert.Throws<ConcurrencyException>(() => _rep.Save(_aggregate, 0));
		}

		[Test]
		public void Should_throw_concurrency_exception_from_session()
		{
			_unitOfWork.Add(_aggregate);
			_aggregate.DoSomething();
			_eventStore.Events.Add(new TestAggregateDidSomething { Id = _aggregate.Id, Version = 4 });
			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Commit());
		}
	}
}