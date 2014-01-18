using System;
using Cqrs.Events;
using Cqrs.Repositories.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateDidSomething : IEvent<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }
	}

	public class TestAggregateDidSomeethingElse : IEvent<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }
	}

	public class TestAggregateDidSomethingHandler : IEventHandler<ISingleSignOnToken, TestAggregateDidSomething>
	{
		public void Handle(TestAggregateDidSomething message)
		{
			lock (message)
			{
				TimesRun++;
			}
		}

		public int TimesRun { get; private set; }
	}
}
