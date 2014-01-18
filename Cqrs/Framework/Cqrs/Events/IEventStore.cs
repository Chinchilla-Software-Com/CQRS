using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public interface IEventStore<TPermissionToken>
	{
		void Save(IEvent<TPermissionToken> @event);

		IEnumerable<IEvent<TPermissionToken>> Get(Guid aggregateId, int fromVersion);
	}
}