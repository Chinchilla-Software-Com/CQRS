using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public interface IEventStore<TAuthenticationToken>
	{
		void Save<T>(IEvent<TAuthenticationToken> @event);

		IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, int fromVersion);
	}
}