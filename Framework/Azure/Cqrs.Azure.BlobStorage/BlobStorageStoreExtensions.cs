#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Azure.BlobStorage.DataStores;
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

		public static IEnumerable<TData> GetByFolder<TData>(this IDataStore<TData> datastore)
			where TData : Entity
		{
			var rawDatastore = (BlobStorageDataStore<TData>)datastore;
			return rawDatastore.GetByFolder();
		}
	}
}