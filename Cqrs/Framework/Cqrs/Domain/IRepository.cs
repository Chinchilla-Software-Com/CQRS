using System;

namespace Cqrs.Domain
{
	public interface IRepository
	{
		void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot;
		TAggregateRoot Get<TAggregateRoot>(Guid aggregateId)
			where TAggregateRoot : IAggregateRoot;
	}
}