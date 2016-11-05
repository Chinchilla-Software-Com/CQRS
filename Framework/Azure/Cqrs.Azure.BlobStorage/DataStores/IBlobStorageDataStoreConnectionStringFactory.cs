#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	public interface IBlobStorageDataStoreConnectionStringFactory : IBlobStorageStoreConnectionStringFactory
	{
		string GetContainerName<TData>()
			where TData : Entity;

		bool IsContainerPublic<TData>()
			where TData : Entity;

		string GetEntityName<TData>()
			where TData : Entity;
	}
}