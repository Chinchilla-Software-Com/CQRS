#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// A factory for getting connection strings and container names with table storage for snapshots.
	/// </summary>
	public interface ITableStorageSnapshotStoreConnectionStringFactory
		: ITableStorageStoreConnectionStringFactory
	{
	}
}