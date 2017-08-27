#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	/// <summary>
	/// A factory for getting connection strings and container names for <see cref="IDataStore{TData}"/> access.
	/// This factory supports reading and writing from separate storage accounts. Specifically you can have as many different storage accounts as you want to configure when writing.
	/// This allows for manual mirroring of data while reading from the fastest/closest location possible.
	/// </summary>
	public interface IBlobStorageDataStoreConnectionStringFactory : IBlobStorageStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets the name of the container.
		/// </summary>
		string GetContainerName<TData>()
			where TData : Entity;

		/// <summary>
		/// Get if the container is public or not.
		/// </summary>
		bool IsContainerPublic<TData>()
			where TData : Entity;

		/// <summary>
		/// Gets the name of the entity that is safe for container use.
		/// </summary>
		string GetEntityName<TData>()
			where TData : Entity;
	}
}