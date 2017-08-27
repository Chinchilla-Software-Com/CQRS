#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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
	/// <summary>
	/// Extension methods for Azure Blob storage.
	/// </summary>
	public static class BlobStorageStoreExtensions
	{
		/// <summary>
		/// Get <typeparamref name="TData"/> by its name.
		/// </summary>
		public static TData GetByName<TData>(this IDataStore<TData> datastore, string name)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByName(name);
		}

		/// <summary>
		/// Get <typeparamref name="TData"/> by its name.
		/// </summary>
		public static TData GetByName<TData>(this IDataStore<TData> datastore, Guid id)
			where TData : Entity, new()
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByName(string.Format("{0}.json", rawDatastore.GenerateFileName(new TData { Rsn = id })));
		}

		/// <summary>
		/// Get all <typeparamref name="TData"/> items in the folder.
		/// </summary>
		public static IEnumerable<TData> GetByFolder<TData>(this IDataStore<TData> datastore, string folderName)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return rawDatastore.GetByFolder(folderName);
		}

		/// <summary>
		/// Get all <typeparamref name="TData"/> items in the folder.
		/// </summary>
		public static IEnumerable<TData> GetByFolder<TData>(this IDataStore<TData> datastore)
			where TData : Entity
		{
			var rawDatastore = (BlobStorageDataStore<TData>)datastore;
			return rawDatastore.GetByFolder();
		}
	}
}