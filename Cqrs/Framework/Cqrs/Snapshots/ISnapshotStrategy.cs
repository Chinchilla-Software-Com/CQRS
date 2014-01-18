using System;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	public interface ISnapshotStrategy<TPermissionScope>
	{
		bool ShouldMakeSnapShot(IAggregateRoot<TPermissionScope> aggregate);

		bool IsSnapshotable(Type aggregateType);
	}
}