using System;
using Cqrs.Bus;
using Cqrs.Authentication;
using cdmdotnet.Logging;
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
			_bus = new InProcessBus<ISingleSignOnToken>(new SingleSignOnTokenValueHelper(), new NullCorrolationIdHelper());
		}

		[Test]
		public void Should_run_handler()
		{
			var handler = new TestAggregateDoSomethingHandler();
			_bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
			_bus.Send(new TestAggregateDoSomething());

			Assert.AreEqual(1,handler.TimesRun);
		}

		[Test]
		public void Should_throw_if_more_handlers()
		{
			var x = new TestAggregateDoSomethingHandler();
			_bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);
			_bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);

			Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
		}

		[Test]
		public void Should_throw_if_no_handlers()
		{
			Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
		}
	}
}