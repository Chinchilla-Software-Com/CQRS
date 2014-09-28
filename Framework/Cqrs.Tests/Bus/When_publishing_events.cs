using Cqrs.Bus;
using Cqrs.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Bus
{
	[TestFixture]
	public class When_publishing_events
	{
		private InProcessBus<ISingleSignOnToken> _bus;

		[SetUp]
		public void Setup()
		{
			_bus = new InProcessBus<ISingleSignOnToken>(new SingleSignOnTokenValueHelper());
		}

		[Test]
		public void Should_publish_to_all_handlers()
		{
			var handler = new TestAggregateDidSomethingHandler();
			_bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
			_bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
			_bus.Publish(new TestAggregateDidSomething());
			Assert.AreEqual(2, handler.TimesRun);
		}

		[Test]
		public void Should_work_with_no_handlers()
		{
			_bus.Publish(new TestAggregateDidSomething());
		}
	}
}