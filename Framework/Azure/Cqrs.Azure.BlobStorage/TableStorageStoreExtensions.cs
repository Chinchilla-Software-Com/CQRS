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
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage
{
	public static class TableStorageStoreExtensions
	{
		public static TData GetByKeyAndRow<TData>(this IDataStore<TData> datastore, Guid rsn)
			where TData : IEntity
		{
			var rawDatastore = (TableStorageStore<EntityTableEntity<TData>, TData>)datastore;
			return rawDatastore.GetByKeyAndRow(rsn).Entity;
		}

		public static IEnumerable<TData> GetByKey<TData>(this IDataStore<TData> datastore)
			where TData : IEntity
		{
			var rawDatastore = (TableStorageStore<EntityTableEntity<TData>, TData>)datastore;
			return rawDatastore.GetByKey().Select(entity => entity.Entity);
		}
	}
}