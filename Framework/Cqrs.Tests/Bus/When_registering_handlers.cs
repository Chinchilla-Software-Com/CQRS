using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Bus
{
	[TestFixture]
	public class When_registering_handlers
	{
		private BusRegistrar _register;
		private TestDependencyResolver _locator;

		[SetUp]
		public void Setup()
		{
			_locator = new TestDependencyResolver(null);
			_register = new BusRegistrar(_locator);
			if (TestHandleRegistrar.HandlerList.Count == 0)
				_register.Register(GetType());
		}

		[Test]
		public void Should_register_all_handlers()
		{
			Assert.AreEqual(6, TestHandleRegistrar.HandlerList.Count);
		}

		[Test]
		public void Should_be_able_to_run_all_handlers()
		{
			TestSagaUnitOfWork.TestSagasWithAddCountGetCountAndCommitCount.Add(Guid.Empty, new Tuple<ISaga<ISingleSignOnToken>, int, int, int>(new TestSaga(_locator, Guid.Empty), 0, 0, 0));
			foreach (var item in TestHandleRegistrar.HandlerList)
			{
				var @event = Activator.CreateInstance(item.Type);
				item.Handler(@event);
			}
			foreach (var handler in _locator.Handlers)
			{
				Assert.That(handler.TimesRun, Is.EqualTo(1));
			}
		}
	}
}
