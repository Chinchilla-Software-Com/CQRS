using System;

namespace Cqrs.Snapshots
{
	public abstract class Snapshot
	{
		public virtual Guid Id { get; set; }

		public virtual int Version { get; set; }
	}
}