using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.WriteModel
{
	public class InMemoryEventStore : IEventStore<ISingleSignOnToken>
	{
		private readonly Dictionary<Guid, List<IEvent<ISingleSignOnToken>>> _inMemoryDB = new Dictionary<Guid, List<IEvent<ISingleSignOnToken>>>();

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Guid aggregateId, int fromVersion)
		{
			List<IEvent<ISingleSignOnToken>> events;
			_inMemoryDB.TryGetValue(aggregateId, out events);
			return events != null ? events.Where(x => x.Version > fromVersion) : new List<IEvent<ISingleSignOnToken>>();
		}

		public void Save(IEvent<ISingleSignOnToken> @event)
		{
			List<IEvent<ISingleSignOnToken>> list;
			_inMemoryDB.TryGetValue(@event.Id, out list);
			if(list == null)
			{
				list = new List<IEvent<ISingleSignOnToken>>();
				_inMemoryDB.Add(@event.Id, list);
			}
			list.Add(@event);
		}
	}
}
