using System;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Tests.Substitutes
{
	public class TestSaga
		: Saga<ISingleSignOnToken>
		, IEventHandler<ISingleSignOnToken, TestAggregateDidSomething>
		, IEventHandler<ISingleSignOnToken, TestAggregateDidSomethingElse>
		, IEventHandler<ISingleSignOnToken, TestAggregateDidSomethingElse2>
	{
		public int DidSomethingCount;

		public bool DidSomething;

		public bool DidSomethingElse;

		public bool DidSomethingElse2;

		public bool Responded;

		private TestSaga(IDependencyResolver dependencyResolver)
			: base(dependencyResolver, dependencyResolver.Resolve<ILogger>())
		{
			ApplyChange(new TestAggregateCreated());
			Version++;
		}

		public TestSaga(IDependencyResolver dependencyResolver, Guid id)
			: base(dependencyResolver, dependencyResolver.Resolve<ILogger>())
		{
			Id = id;
			ApplyChange(new TestAggregateCreated());
			Version++;
		}

		#region Implementation of IMessageHandler<in TestAggregateDidSomething>

		public void Handle(TestAggregateDidSomething message)
		{
			// This could happen out of order in this test
			if (DidSomethingElse2)
			{
				CommandPublisher.Publish(new TestAggregateDoSomething3());
				// This is a testing variable
				Responded = true;
			}
			ApplyChange(message);
		}

		#endregion

		#region Implementation of IMessageHandler<in TestAggregateDidSomethingElse>

		public void Handle(TestAggregateDidSomethingElse message)
		{
			ApplyChange(message);
		}

		#endregion

		#region Implementation of IMessageHandler<in TestAggregateDidSomethingElse>

		public void Handle(TestAggregateDidSomethingElse2 message)
		{
			// This could happen out of order in this test
			if (DidSomething)
			{
				CommandPublisher.Publish(new TestAggregateDoSomething3());
				// This is a testing variable
				Responded = true;
			}
			ApplyChange(message);
		}

		#endregion

		public void Apply(TestAggregateDidSomething e)
		{
			DidSomethingCount++;
			DidSomething = true;
		}

		public void Apply(TestAggregateDidSomethingElse e)
		{
			DidSomethingElse = true;
		}
	}
}
