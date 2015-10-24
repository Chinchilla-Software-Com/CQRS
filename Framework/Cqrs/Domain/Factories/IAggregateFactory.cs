using System;

namespace Cqrs.Domain.Factories
{
	public interface IAggregateFactory
	{
		TAggregate CreateAggregate<TAggregate>(Guid? rsn = null);
	}
}