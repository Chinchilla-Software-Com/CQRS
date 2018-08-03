using System;
using Cqrs.Domain;
using Cqrs.Snapshots;

namespace Cqrs.Tests.Substitutes
{
	public class TestInMemorySnapshotStore : ISnapshotStore 
	{
		public Snapshot Get<TAggregateRoot>(Guid id)
		{
			return _snapshot;
		}

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to find a snapshot for.</param>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get the most recent <see cref="Snapshot"/> of.</param>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		public Snapshot Get(Type aggregateRootType, Guid id)
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