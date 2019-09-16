#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.DataStores;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	/// <summary>
	/// A factory for getting connection strings and container names for <see cref="IDataStore{TData}"/> access.
	/// This factory supports reading and writing from separate storage accounts. Specifically you can have as many different storage accounts as you want to configure when writing.
	/// This allows for manual mirroring of data while reading from the fastest/closest location possible.
	/// </summary>
	public interface ITableStorageDataStoreConnectionStringFactory : ITableStorageStoreConnectionStringFactory
	{
		/// <summary>
		/// Generates the name of the table for <typeparamref name="TData"/> that matches naming rules for Azure Storage.
		/// </summary>
		/// <typeparam name="TData">The <see cref="Type"/> of data to read or write.</typeparam>
		/// <remarks>https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/</remarks>
		string GetTableName<TData>();
	}
}