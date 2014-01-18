using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Repositories.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestInMemoryEventStore : IEventStore<ISingleSignOnToken>
	{
		public readonly List<IEvent<ISingleSignOnToken>> Events = new List<IEvent<ISingleSignOnToken>>();

		public void Save(IEvent<ISingleSignOnToken> @event)
		{
			lock(Events)
			{
				Events.Add(@event);
			}
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Guid aggregateId, int fromVersion)
		{
			lock(Events)
			{
				return Events.Where(x => x.Version > fromVersion && x.Id == aggregateId).OrderBy(x => x.Version).ToList();
			}
		}
	}
}