using System;
using System.Collections.Generic;
using Cqrs.Events;
using Cqrs.Repositories.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestEventStoreWithBugs : IEventStore<ISingleSignOnToken>
	{
		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Guid aggregateId, int version)
		{
			if (aggregateId == Guid.Empty)
			{
				return new List<IEvent<ISingleSignOnToken>>();
			}

			return new List<IEvent<ISingleSignOnToken>>
				{
					new TestAggregateDidSomething {Id = aggregateId, Version = 3},
					new TestAggregateDidSomething {Id = aggregateId, Version = 2},
					new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 1},
				};

		}

		public void Save(IEvent<ISingleSignOnToken> eventDescriptor)
		{
		}

	}
}