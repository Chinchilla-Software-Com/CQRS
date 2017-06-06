using System;

namespace Cqrs.Domain.Factories
{
	public interface IAggregateFactory
	{
		TAggregate Create<TAggregate>(Guid? rsn = null, bool tryDependencyResolutionFirst = true);

		object Create(Type aggregateType, Guid? rsn = null, bool tryDependencyResolutionFirst = true);
	}
}