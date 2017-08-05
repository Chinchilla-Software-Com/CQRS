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
	/// <summary>
	/// A <see cref="BlobStorage.DataStores.TableStorageDataStore{TData}"/> that uses Azure Storage for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="TableEntity"/> Azure Table Storage will contain.</typeparam>
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

		/// <summary>
		/// Creates a new instance of <see cref="DynamicTableEntity"/> populating it with the provided <paramref name="data"/>
		/// </summary>
		/// <param name="data">The data to store in <see cref="DynamicTableEntity.Properties"/>.</param>
		protected override ITableEntity CreateTableEntity(TData data)
		{
			var tableEntity = new EntityTableEntity<TData>(data);
			//Flatten object of type TData and convert it to EntityProperty Dictionary
#pragma warning disable 0436
			Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(data, new OperationContext());
#pragma warning restore 0436
			// Create a DynamicTableEntity and set its PK and RK
			DynamicTableEntity dynamicTableEntity = new DynamicTableEntity(tableEntity.PartitionKey, tableEntity.RowKey)
			{
				Properties = flattenedProperties
			};

			return dynamicTableEntity;
		}

		/// <summary>
		/// Gets a <see cref="TableOperation"/> that calls <see cref="TableOperation.Retrieve{DynamicTableEntity}(string,string,System.Collections.Generic.List{string})"/>
		/// read for updating.
		/// </summary>
		/// <param name="data">The data containing the <see cref="IEntity.Rsn"/> property populated.</param>
		protected override TableOperation GetUpdatableTableEntity(TData data)
		{
			return TableOperation.Retrieve<DynamicTableEntity>(data.GetType().FullName, data.Rsn.ToString("N"));
		}

		#endregion

		#region Overrides of TableStorageStore<EntityTableEntity<TData>,TData>

		/// <summary>
		/// Extracts <see cref="TableResult.Result"/> of the provided <paramref name="retrievedResult"/>
		/// If <see cref="TableResult.Result"/> is NOT a <see cref="DynamicTableEntity"/> <see cref="TableStorageStore{TData,TCollectionItemData}.ReplaceValues"/> is called.
		/// Otherwise <see cref="TableResult.Result"/> is a <see cref="DynamicTableEntity"/>
		/// and <see cref="DynamicTableEntity.Properties"/> are replaced with values from <paramref name="data"/>.
		/// </summary>
		/// <param name="retrievedResult">The existing data to update.</param>
		/// <param name="data">The new data to store.</param>
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
#pragma warning disable 0436
			Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(data.Entity, new OperationContext());
#pragma warning restore 0436
			dynamicTableEntity.Properties = flattenedProperties;

			return dynamicTableEntity;
		}

		/// <summary>
		/// Retrieves the data from Azure Storage 
		/// If the data is NOT a <see cref="DynamicTableEntity"/> <see cref="TableStorageStore{TData,TCollectionItemData}.GetByKeyAndRow"/> is called.
		/// Otherwise <see cref="TableResult.Result"/> is a <see cref="DynamicTableEntity"/>
		/// and <see cref="DynamicTableEntity.Properties"/> is converted back to <see cref="EntityTableEntity{TData}"/>.
		/// </summary>
		public override EntityTableEntity<TData> GetByKeyAndRow(Guid rsn)
		{
			TableOperation searchQuery = TableOperation.Retrieve<DynamicTableEntity>(typeof(TData).FullName, rsn.ToString("N"));

			TableResult searchResult = ReadableSource.Execute(searchQuery);

			var dynamicTableEntity = searchResult.Result as DynamicTableEntity;
			if (dynamicTableEntity == null)
				return base.GetByKeyAndRow(rsn);

			//Convert the DynamicTableEntity back to original complex object.
#pragma warning disable 0436
			TData result = EntityPropertyConverter.ConvertBack<TData>(dynamicTableEntity.Properties, new OperationContext());
#pragma warning restore 0436
			return new EntityTableEntity<TData>(result);
		}

		/// <summary>
		/// Retrieves the data from Azure Storage using <see cref="TableStorageStore{TData,TCollectionItemData}.Collection"/>.
		/// </summary>
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

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public override void Update(TData data)
		{
			DynamicTableEntity dynamicTableEntity = CreateTableEntity(data) as DynamicTableEntity;
			if (dynamicTableEntity == null)
			{
				base.Update(data);
				return;
			}
			//Convert the DynamicTableEntity back to original complex object.
#pragma warning disable 0436
			TData result = EntityPropertyConverter.ConvertBack<TData>(dynamicTableEntity.Properties, new OperationContext());
#pragma warning restore 0436
			Update(new EntityTableEntity<TData>(result));
		}
	}
}