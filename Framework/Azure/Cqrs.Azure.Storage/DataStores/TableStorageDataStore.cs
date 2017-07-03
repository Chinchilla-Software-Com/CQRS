#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using Cqrs.Azure.BlobStorage;
using Cqrs.Azure.BlobStorage.DataStores;
using Cqrs.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.Storage.DataStores
{
	public class TableStorageDataStore<TData>
		: BlobStorage.DataStores.TableStorageDataStore<TData>
		where TData : Entity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
		/// </summary>
		public TableStorageDataStore(ILogger logger, ITableStorageDataStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
			: base(logger, tableStorageDataStoreConnectionStringFactory)
		{
		}

		#region Overrides of TableStorageStore<TData>

		protected override ITableEntity CreateTableEntity(TData data)
		{
			var tableEntity = new EntityTableEntity<TData>(data);
			//Flatten object of type TData and convert it to EntityProperty Dictionary
			Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(data, new OperationContext());

			// Create a DynamicTableEntity and set its PK and RK
			DynamicTableEntity dynamicTableEntity = new DynamicTableEntity(tableEntity.PartitionKey, tableEntity.RowKey)
			{
				Properties = flattenedProperties
			};

			return dynamicTableEntity;
		}

		protected override TableOperation GetUpdatableTableEntity(TData data)
		{
			return TableOperation.Retrieve<DynamicTableEntity>(data.GetType().FullName, data.Rsn.ToString("N"));
		}

		#endregion

		#region Overrides of TableStorageStore<EntityTableEntity<TData>,TData>

		protected override ITableEntity ReplaceValues(TableResult retrievedResult, EntityTableEntity<TData> data)
		{
			ITableEntity tableEntity = (ITableEntity)retrievedResult.Result;
			// Events aren't updated
			var dynamicTableEntity = tableEntity as DynamicTableEntity;
			if (dynamicTableEntity == null)
			{
				base.ReplaceValues(retrievedResult, data);
				return tableEntity;
			}

			//Flatten object of type TData and convert it to EntityProperty Dictionary
			Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(data.Entity, new OperationContext());
			dynamicTableEntity.Properties = flattenedProperties;

			return dynamicTableEntity;
		}

		public override EntityTableEntity<TData> GetByKeyAndRow(Guid rsn)
		{
			TableOperation searchQuery = TableOperation.Retrieve<DynamicTableEntity>(typeof(TData).FullName, rsn.ToString("N"));

			TableResult searchResult = ReadableSource.Execute(searchQuery);

			var dynamicTableEntity = searchResult.Result as DynamicTableEntity;
			if (dynamicTableEntity == null)
				return base.GetByKeyAndRow(rsn);

			//Convert the DynamicTableEntity back to original complex object.
			TData result = EntityPropertyConverter.ConvertBack<TData>(dynamicTableEntity.Properties, new OperationContext());
			return new EntityTableEntity<TData>(result);
		}

		public override IEnumerable<EntityTableEntity<TData>> GetByKey()
		{
			// Create the table query.
			var rangeQuery = Collection.Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(typeof(TData).FullName))
			);

			IEnumerable<EntityTableEntity<TData>> results = ReadableSource.ExecuteQuery(rangeQuery);

			return results;
		}

		#endregion

		public override void Update(TData data)
		{
			DynamicTableEntity dynamicTableEntity = CreateTableEntity(data) as DynamicTableEntity;
			if (dynamicTableEntity == null)
			{
				base.Update(data);
				return;
			}
			//Convert the DynamicTableEntity back to original complex object.
			TData result = EntityPropertyConverter.ConvertBack<TData>(dynamicTableEntity.Properties, new OperationContext());
			Update(new EntityTableEntity<TData>(result));
		}
	}
}