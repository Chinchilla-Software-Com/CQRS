using System;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	public interface ISnapshotStrategy<TPermissionToken>
	{
		bool ShouldMakeSnapShot(IAggregateRoot<TPermissionToken> aggregate);

		bool IsSnapshotable(Type aggregateType);
	}
}