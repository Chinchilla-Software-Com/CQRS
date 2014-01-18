using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public interface IEventStore<TPermissionScope>
	{
		void Save(IEvent<TPermissionScope> @event);

		IEnumerable<IEvent<TPermissionScope>> Get(Guid aggregateId, int fromVersion);
	}
}