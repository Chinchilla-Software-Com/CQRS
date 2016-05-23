using System;
using Cqrs.Bus;
using Cqrs.Authentication;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Configuration;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Bus
{
	[TestFixture]
	public class When_sending_command
	{
		private InProcessBus<ISingleSignOnToken> _bus;

		[SetUp]
		public void Setup()
		{
			_bus = new InProcessBus<ISingleSignOnToken>(new SingleSignOnTokenValueHelper(), new NullCorrelationIdHelper(), new TestDependencyResolver(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new ConfigurationManager());
		}

		[Test]
		public void Should_run_handler()
		{
			var handler = new TestAggregateDoSomethingHandler();
			_bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle, handler.GetType());
			_bus.Send(new TestAggregateDoSomething());

			Assert.AreEqual(1,handler.TimesRun);
		}

		[Test]
		public void Should_throw_if_more_handlers()
		{
			var x = new TestAggregateDoSomethingHandler();
			_bus.RegisterHandler<TestAggregateDoSomething>(x.Handle, x.GetType());
			_bus.RegisterHandler<TestAggregateDoSomething>(x.Handle, x.GetType());

			Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
		}

		[Test]
		public void Should_throw_if_no_handlers()
		{
			Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
		}
	}
}