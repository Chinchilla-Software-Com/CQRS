using System;
using Cqrs.Snapshots;

namespace Cqrs.Tests.Substitutes
{
    public class TestInMemorySnapshotStore : ISnapshotStore 
    {
		public Snapshot Get<TAggregateRoot>(Guid id)
        {
            return _snapshot;
        }

        public void Save(Snapshot snapshot)
        {
            if(snapshot.Version == 0)
                FirstSaved = true;
            SavedVersion = snapshot.Version;
            _snapshot = snapshot;
        }

        private Snapshot _snapshot;
        public int SavedVersion { get; private set; }
        public bool FirstSaved { get; private set; }
    }
}