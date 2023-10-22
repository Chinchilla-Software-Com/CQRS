#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cqrs.Azure.Storage.DataStores;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// Extension methods for Azure Blob storage.
	/// </summary>
	public static class BlobStorageStoreExtensions
	{
		/// <summary>
		/// Get <typeparamref name="TData"/> by its name.
		/// </summary>
		public static
#if NET472
			TData GetByName
#else
			async Task<TData> GetByNameAsync
#endif
				<TData>(this IDataStore<TData> datastore, string name)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return
#if NET472
				rawDatastore.GetByName
#else
				await rawDatastore.GetByNameAsync
#endif
			 (name);
		}

		/// <summary>
		/// Get <typeparamref name="TData"/> by its name.
		/// </summary>
		public static
#if NET472
			TData GetByName
#else
			async Task<TData> GetByNameAsync
#endif
				<TData>(this IDataStore<TData> datastore, Guid id)
			where TData : Entity, new()
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return
#if NET472
				rawDatastore.GetByName
#else
				await rawDatastore.GetByNameAsync
#endif
			(string.Format("{0}.json", rawDatastore.GenerateFileName(new TData { Rsn = id })));
		}

		/// <summary>
		/// Get all <typeparamref name="TData"/> items in the folder.
		/// </summary>
		public static
#if NET472
			IEnumerable<TData> GetByFolder
#else
			async Task<IEnumerable<TData>> GetByFolderAsync
#endif
				<TData>(this IDataStore<TData> datastore, string folderName)
		{
			var rawDatastore = (BlobStorageStore<TData>)datastore;
			return
#if NET472
				rawDatastore.GetByFolder
#else
				await rawDatastore.GetByFolderAsync
#endif
				 (folderName);
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