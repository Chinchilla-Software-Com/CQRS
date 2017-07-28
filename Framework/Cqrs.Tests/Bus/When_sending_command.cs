using System;
using Cqrs.Bus;
using Cqrs.Authentication;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Exceptions;
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
			_bus = new InProcessBus<ISingleSignOnToken>(new AuthenticationTokenHelper(new ThreadedContextItemCollectionFactory()), new NullCorrelationIdHelper(), new TestDependencyResolver(null), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new ConfigurationManager(), new BusHelper(new ConfigurationManager()));
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

			Assert.Throws<MultipleCommandHandlersRegisteredException>(() => _bus.Send(new TestAggregateDoSomething()));
		}

		[Test]
		public void Should_throw_if_no_handlers()
		{
			Assert.Throws<NoCommandHandlerRegisteredException>(() => _bus.Send(new TestAggregateDoSomething2()));
		}

		[Test]
		public void Has_no_handles_should_not_throw_due_to_settings()
		{
			_bus.Send(new TestAggregateDoSomething3());
		}
	}
}