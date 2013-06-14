using System;

namespace Cqrs.Snapshots
{
    public interface ISnapshotStore
    {
        Snapshot Get(Guid id);
        void Save(Snapshot snapshot);
    }
}
