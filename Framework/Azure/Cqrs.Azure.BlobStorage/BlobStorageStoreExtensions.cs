using System;
using System.Collections.Generic;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage
{
	public static class BlobStorageStoreExtensions
	{
		public static TData GetByName<TData>(this IDataStore<TData> datastore, string name)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByName(name);
		}

		public static TData GetByName<TData>(this IDataStore<TData> datastore, Guid id)
			where TData : Entity, new()
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByName(string.Format("{0}.json", rawDatastore.GenerateFileName(new TData { Rsn = id })));
		}

		public static IEnumerable<TData> GetByFolder<TData>(this IDataStore<TData> datastore, string folderName)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByFolder(folderName);
		}
	}
}