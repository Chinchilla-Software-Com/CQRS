using System;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain.Saga
{
	[TestFixture]
	public class When_getting_a_saga
	{
		private ISagaUnitOfWork<ISingleSignOnToken> _unitOfWork;

		private TestDependencyResolver _dependencyResolver;

		private TestCommandPublisher _commandPublisher;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestEventStore();
			_commandPublisher = new TestCommandPublisher();
			_dependencyResolver = new TestDependencyResolver(eventStore, _commandPublisher);
			var sagaFactory = new AggregateFactory(_dependencyResolver, _dependencyResolver.Resolve<ILogger>());
			var testEventPublisher = new TestEventPublisher();
			_unitOfWork = new SagaUnitOfWork<ISingleSignOnToken>(new SagaRepository<ISingleSignOnToken>(sagaFactory, eventStore, testEventPublisher, null, new NullCorrelationIdHelper()));
		}

		[Test]
		public void Should_get_saga_from_eventstore()
		{
			_dependencyResolver.UseTestEventStoreGuid = false;
			var saga = _unitOfWork.Get<TestSaga>(Guid.NewGuid());
			Assert.NotNull(saga);
		}

		[Test]
		public void Should_apply_events()
		{
			var saga = _unitOfWork.Get<TestSaga>(Guid.NewGuid());
			Assert.AreEqual(2, saga.DidSomethingCount);
		}

		[Test]
		public void Should_not_send_command()
		{
			var saga = _unitOfWork.Get<TestSaga>(Guid.NewGuid());
			Assert.AreEqual(0, _commandPublisher.Published);
		}

		[Test]
		public void Should_send_command_on_new_event()
		{
			var saga = _unitOfWork.Get<TestSaga>(Guid.NewGuid());
			saga.Handle(new TestAggregateDidSomethingElse2());
			Assert.AreEqual(1, _commandPublisher.Published);
		}

		[Test]
		public void Should_fail_if_saga_do_not_exist()
		{
			_dependencyResolver.UseTestEventStoreGuid = true;
			Assert.Throws<SagaNotFoundException<TestSaga, ISingleSignOnToken>>(() => _unitOfWork.Get<TestSaga>(Guid.Empty));
		}

		[Test]
		public void Should_track_changes()
		{
			var agg = new TestSaga(_dependencyResolver, Guid.NewGuid());
			_unitOfWork.Add(agg);
			var saga = _unitOfWork.Get<TestSaga>(agg.Id);
			Assert.AreEqual(agg, saga);
		}

		[Test]
		public void Should_get_from_session_if_tracked()
		{
			var id = Guid.NewGuid();
			_dependencyResolver.NewAggregateGuid = id;
			var saga = _unitOfWork.Get<TestSaga>(id);
			var saga2 = _unitOfWork.Get<TestSaga>(id);

			Assert.AreEqual(saga, saga2);
		}

		[Test]
		public void Should_throw_concurrency_exception_if_tracked()
		{
			var id = Guid.NewGuid();
			_unitOfWork.Get<TestSaga>(id);

			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Get<TestSaga>(id, 100));
		}

		[Test]
		public void Should_get_correct_version()
		{
			var id = Guid.NewGuid();
			var saga = _unitOfWork.Get<TestSaga>(id);

			Assert.AreEqual(4, saga.Version);
		}

		[Test]
		public void Should_throw_concurrency_exception()
		{
			var id = Guid.NewGuid();
			Assert.Throws<ConcurrencyException>(() => _unitOfWork.Get<TestSaga>(id, 1));
		}
	}
}