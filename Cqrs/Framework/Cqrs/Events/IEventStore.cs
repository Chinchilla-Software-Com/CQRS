using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public interface IEventStore<TAuthenticationToken>
	{
		void Save<T>(IEvent<TAuthenticationToken> @event);

		void Save(IEvent<TAuthenticationToken> @event, Type aggregateRootType);

		IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, int fromVersion);
	}
}