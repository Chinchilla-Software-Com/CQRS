using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IRepository
	{
		void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot;
		TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent> events = null)
			where TAggregateRoot : IAggregateRoot;
	}
}