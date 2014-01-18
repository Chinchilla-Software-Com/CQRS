using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IRepository<TPermissionToken>
	{
		void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TPermissionToken>;

		TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TPermissionToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionToken>;
	}
}