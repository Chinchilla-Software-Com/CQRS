using System;
using Cqrs.Domain;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateNoParameterLessConstructor : AggregateRoot<ISingleSignOnToken>
	{
		public TestAggregateNoParameterLessConstructor(int i, Guid? id = null)
		{
			Id = id ?? Guid.NewGuid();
		}

		public void DoSomething()
		{
			ApplyChange(new TestAggregateDidSomething());
		}
	}
}