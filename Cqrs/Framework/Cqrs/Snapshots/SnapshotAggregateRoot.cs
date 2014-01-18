using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	public abstract class SnapshotAggregateRoot<TPermissionToken, TSnapshot> : AggregateRoot<TPermissionToken>
		where TSnapshot : Snapshot
	{
		public TSnapshot GetSnapshot()
		{
			var snapshot = CreateSnapshot();
			snapshot.Id = Id;
			return snapshot;
		}

		public void Restore(TSnapshot snapshot)
		{
			Id = snapshot.Id;
			Version = snapshot.Version;
			RestoreFromSnapshot(snapshot);
		}

		protected abstract TSnapshot CreateSnapshot();

		protected abstract void RestoreFromSnapshot(TSnapshot snapshot);
	}

}
