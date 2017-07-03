#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Azure.BlobStorage.DataStores
{
	public interface ITableStorageDataStoreConnectionStringFactory : ITableStorageStoreConnectionStringFactory
	{
		string GetTableName<TData>();
	}
}