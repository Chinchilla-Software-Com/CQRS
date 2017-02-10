using System;

namespace Cqrs.Domain.Factories
{
	public interface IAggregateFactory
	{
		TAggregate CreateAggregate<TAggregate>(Guid? rsn = null, bool tryDependencyResolutionFirst = true);

		object CreateAggregate(Type aggregateType, Guid? rsn = null, bool tryDependencyResolutionFirst = true);
	}
}