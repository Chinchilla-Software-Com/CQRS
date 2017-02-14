using System;
using Cqrs.Authentication;
using Cqrs.Snapshots;

namespace Cqrs.Tests.Substitutes
{
	public class TestSnapshotAggregate : SnapshotAggregateRoot<ISingleSignOnToken, TestSnapshotAggregateSnapshot>
	{
		public TestSnapshotAggregate(Guid? id = null)
		{
			Id = id ?? Guid.NewGuid();
		}

		public bool Restored { get; private set; }
		public bool Loaded { get; private set; }
		public int Number { get; private set; }

		protected override TestSnapshotAggregateSnapshot CreateSnapshot()
		{
			return new TestSnapshotAggregateSnapshot {Number = Number};
		}

		protected override void RestoreFromSnapshot(TestSnapshotAggregateSnapshot snapshot)
		{
			Number = snapshot.Number;
			Restored = true;
		}

		private void Apply(TestAggregateDidSomething e)
		{
			Loaded = true;
			Number++;
		}

		public void DoSomething()
		{
			ApplyChange(new TestAggregateDidSomething());
		}
	}

	public class TestSnapshotAggregateSnapshot : Snapshot
	{
		public int Number { get; set; }
	}
}
