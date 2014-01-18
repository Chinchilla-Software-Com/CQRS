using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IRepository<TPermissionScope>
	{
		void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>;

		TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TPermissionScope>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>;
	}
}