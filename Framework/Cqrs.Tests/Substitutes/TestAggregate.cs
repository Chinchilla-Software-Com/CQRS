using System;
using Cqrs.Domain;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregate : AggregateRoot<ISingleSignOnToken>
	{
		private TestAggregate()
		{
			ApplyChange(new TestAggregateCreated());
			Version++;
		}

		public TestAggregate(Guid id)
		{
			Id = id;
			ApplyChange(new TestAggregateCreated());
			Version++;
		}

		public int DidSomethingCount;

		public void DoSomething()
		{
			ApplyChange(new TestAggregateDidSomething());
		}

		public void DoSomethingElse()
		{
			ApplyChange(new TestAggregateDidSomethingElse());
		}

		public void Apply(TestAggregateDidSomething e)
		{
			DidSomethingCount++;
		}
	}
}
