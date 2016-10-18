using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestSnapshotEventStore : TestEventStore
	{
		public override IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			if (aggregateId == EmptyGuid || aggregateId == Guid.Empty)
			{
				return new List<IEvent<ISingleSignOnToken>>();
			}

			return new List<IEvent<ISingleSignOnToken>>
			{
				new TestAggregateDidSomething {Id = aggregateId, Version = 1},
				new TestAggregateDidSomethingElse {Id = aggregateId, Version = 2},
				new TestAggregateDidSomething {Id = aggregateId, Version = 3}
			}
			.Where(x => x.Version > fromVersion);
		}
	}
}