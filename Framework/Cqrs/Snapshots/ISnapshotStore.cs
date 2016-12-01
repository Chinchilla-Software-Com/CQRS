#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Snapshots
{
	public interface ISnapshotStore
	{
		Snapshot Get(Guid id);

		void Save(Snapshot snapshot);
	}
}