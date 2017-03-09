#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

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
			GetContainerName = () => string.Format("{0}_v2", tableStorageDataStoreConnectionStringFactory.GetTableName<TData>());
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

		protected override void ReplaceValues(TableResult retrievedResult, EntityTableEntity<TData> data)
		{
			ITableEntity tableEntity = (ITableEntity)retrievedResult.Result;
			// Events aren't updated
			var dynamicTableEntity = tableEntity as DynamicTableEntity;
			if (dynamicTableEntity == null)
			{
				base.ReplaceValues(retrievedResult, data);
				return;
			}

			//Flatten object of type TData and convert it to EntityProperty Dictionary
			Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(data, new OperationContext());
			dynamicTableEntity.Properties = flattenedProperties;
		}

		#endregion
	}
}