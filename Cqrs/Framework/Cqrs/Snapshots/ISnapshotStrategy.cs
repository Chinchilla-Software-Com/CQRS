using System;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(IAggregateRoot aggregate);
        bool IsSnapshotable(Type aggregateType);
    }
}