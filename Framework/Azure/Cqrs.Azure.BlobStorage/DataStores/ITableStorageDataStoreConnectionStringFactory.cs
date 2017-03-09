#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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