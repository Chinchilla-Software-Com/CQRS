using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestEventStoreWithBugs : IEventStore<ISingleSignOnToken>
	{
		public void Save(Type aggregateRootType, IEvent<ISingleSignOnToken> @event)
		{
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof (T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			if (aggregateId == Guid.Empty)
			{
				return new List<IEvent<ISingleSignOnToken>>();
			}

			return new List<IEvent<ISingleSignOnToken>>
			{
				new TestAggregateDidSomething {Id = aggregateId, Version = 3},
				new TestAggregateDidSomething {Id = aggregateId, Version = 4},
				new TestAggregateDidSomethingElse {Id = aggregateId, Version = 2}
			}
			.Where(x => x.Version > fromVersion);
		}

		public IEnumerable<EventData> Get(Guid correlationId)
		{
			return Enumerable.Empty<EventData>();
		}

		public void Save<T>(IEvent<ISingleSignOnToken> eventDescriptor)
		{
		}
	}
}