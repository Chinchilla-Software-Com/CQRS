#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Azure.BlobStorage
{
	/// <summary>
	/// A factory for getting connection strings and container names with table storage.
	/// </summary>
	public interface ITableStorageStoreConnectionStringFactory : IStorageStoreConnectionStringFactory
	{
		/// <summary>
		/// Returns the name of the base contain to be used.
		/// </summary>
		string GetBaseContainerName();
	}
}