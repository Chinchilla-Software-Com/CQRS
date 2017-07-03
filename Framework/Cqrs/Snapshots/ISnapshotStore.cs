#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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