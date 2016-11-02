using System.Collections.Generic;
using Cqrs.DataStores;

namespace Cqrs.Azure.BlobStorage
{
	public static class BlobStorageStoreExtensions
	{
		public static TData GetByName<TData>(this IDataStore<TData> datastore, string name)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByName(name);
		}

		public static IEnumerable<TData> GetByFolder<TData>(this IDataStore<TData> datastore, string folderName)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByFolder(folderName);
		}
	}
}