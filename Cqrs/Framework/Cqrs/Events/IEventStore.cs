using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public interface IEventStore<TAuthenticationToken>
	{
		void Save(IEvent<TAuthenticationToken> @event);

		IEnumerable<IEvent<TAuthenticationToken>> Get(Guid aggregateId, int fromVersion);
	}
}