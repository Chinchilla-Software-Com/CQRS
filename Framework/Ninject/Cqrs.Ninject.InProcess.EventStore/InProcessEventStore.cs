using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;

namespace Cqrs.Ninject.InProcess.EventStore
{
	public class InProcessEventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> InMemoryDb { get; private set; }

		public InProcessEventStore()
		{
			InMemoryDb = new Dictionary<Guid, IList<IEvent<TAuthenticationToken>>>();
		}

		public void Save(IEvent<TAuthenticationToken> @event, Type aggregateRootType)
		{
			IList<IEvent<TAuthenticationToken>> list;
			InMemoryDb.TryGetValue(@event.Id, out list);
			if (list == null)
			{
				list = new List<IEvent<TAuthenticationToken>>();
				InMemoryDb.Add(@event.Id, list);
			}
			list.Add(@event);
		}

		public IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			IList<IEvent<TAuthenticationToken>> events;
			InMemoryDb.TryGetValue(aggregateId, out events);
			return events != null
				? events.Where(x => x.Version > fromVersion)
				: new List<IEvent<TAuthenticationToken>>();
		}

		public void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(@event, typeof(T));
		}
	}
}