using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestInMemoryEventStore : IEventStore<ISingleSignOnToken>
	{
		public readonly List<IEvent<ISingleSignOnToken>> Events = new List<IEvent<ISingleSignOnToken>>();

		public void Save<T>(IEvent<ISingleSignOnToken> @event)
		{
			Save(typeof(T), @event);
		}

		public void Save(Type aggregateRootType, IEvent<ISingleSignOnToken> @event)
		{
			lock(Events)
			{
				Events.Add(@event);
			}
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof (T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			lock (Events)
			{
				return Events.Where(x => x.Version > fromVersion && x.Id == aggregateId).OrderBy(x => x.Version).ToList();
			}
		}
	}
}