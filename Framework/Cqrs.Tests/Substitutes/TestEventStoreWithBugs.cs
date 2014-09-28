using System;
using System.Collections.Generic;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestEventStoreWithBugs : IEventStore<ISingleSignOnToken>
	{
		public void Save(IEvent<ISingleSignOnToken> @event, Type aggregateRootType)
		{
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
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

		public void Save<T>(IEvent<ISingleSignOnToken> eventDescriptor)
		{
		}
	}
}