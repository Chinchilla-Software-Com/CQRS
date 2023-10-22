#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Data.Tables;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// Extension methods for Azure Table storage.
	/// </summary>
	public static class TableStorageStoreExtensions
	{
		/// <summary>
		/// Retrieves the data from Azure Storage by it's <see cref="ITableEntity.PartitionKey"/> and <see cref="ITableEntity.RowKey"/>.
		/// </summary>
		public static TData GetByKeyAndRow<TData>(this IDataStore<TData> datastore, Guid rsn)
			where TData : IEntity
		{
			var rawDatastore = (TableStorageStore<EntityTableEntity<TData>, TData>)datastore;
			return rawDatastore.GetByKeyAndRow(rsn).Entity;
		}

		/// <summary>
		/// Retrieves the data from Azure Storage by it's <see cref="TableEntity.PartitionKey"/>.
		/// </summary>
		public static IEnumerable<TData> GetByKey<TData>(this IDataStore<TData> datastore)
			where TData : IEntity
		{
			var rawDatastore = (TableStorageStore<EntityTableEntity<TData>, TData>)datastore;
			return rawDatastore.GetByKey().Select(entity => entity.Entity);
		}
	}
}