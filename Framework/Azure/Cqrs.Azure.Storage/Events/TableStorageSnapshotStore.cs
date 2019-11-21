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
using Chinchilla.Logging;
using Cqrs.Azure.BlobStorage;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Snapshots;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.Storage.Events
{
	/// <summary>
	/// An Azure Storage based <see cref="SnapshotStore"/>.
	/// </summary>
	public class TableStorageSnapshotStore
		: BlobStorage.Events.TableStorageSnapshotStore
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageSnapshotStore"/> class using the specified container.
		/// </summary>
		public TableStorageSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder, ITableStorageSnapshotStoreConnectionStringFactory tableStorageSnapshotStoreConnectionStringFactory)
			: base(configurationManager, eventDeserialiser, logger, correlationIdHelper, snapshotBuilder, tableStorageSnapshotStoreConnectionStringFactory, (logger1, tableStorageSnapshotStoreConnectionStringFactory1) => new RawTableStorageSnapshotStorer(logger1, tableStorageSnapshotStoreConnectionStringFactory1))
		{
		}

		#region Overrides of SnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected override Snapshot Get(Type aggregateRootType, string streamName)
		{
			// Create the table query.
			var rangeQuery = new TableQuery<DynamicTableEntity>().Where
			(
				TableQuery.CombineFilters
				(
					TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(streamName)),
					TableOperators.And,
					TableQuery.GenerateFilterCondition("AggregateId", QueryComparisons.Equal, streamName)
				)
			);

			var operationContext = new OperationContext();
			Snapshot result = TableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
#pragma warning disable 0436
				.Select(eventData => EntityPropertyConverter.ConvertBack<EventData>(eventData.Properties, operationContext))
#pragma warning restore 0436
.Where(eventData => eventData.AggregateId == streamName)
				.OrderByDescending(eventData => eventData.Version)
				.Take(1)
				.Select(EventDeserialiser.Deserialise)
				.SingleOrDefault();

			return result;
		}

		#endregion

		/// <summary>
		/// An Azure Storage based <see cref="TableStorageStore{TData,TCollectionItemData}"/>.
		/// </summary>
		public class RawTableStorageSnapshotStorer
			: RawTableStorageSnapshotStore
		{
			/// <summary>
			/// Initializes a new instance of the RawTableStorageSnapshotStorer class using the specified container.
			/// </summary>
			public RawTableStorageSnapshotStorer(ILogger logger, ITableStorageSnapshotStoreConnectionStringFactory tableStorageSnapshotStoreConnectionStringFactory)
				: base(logger, tableStorageSnapshotStoreConnectionStringFactory)
			{
			}

			#region Overrides of StorageStore<EventData,CloudTable>

			/// <summary>
			/// The value differs from RawTableStorageEventStore.GetSafeSourceName(string) in that it appends "V2" to the end of the name.
			/// </summary>
			protected override string GetSafeSourceName(string sourceName)
			{
				string tableName = base.GetSafeSourceName(sourceName);
				if (tableName.Length > 34)
					tableName = tableName.Substring(tableName.Length - 34);
				return string.Format("{0}V2", tableName);
			}

			#endregion

			#region Overrides of TableStorageStore<EventData>

			/// <summary>
			/// Creates a new <see cref="DynamicTableEntity"/> copying the provided <paramref name="data"/>
			/// into <see cref="DynamicTableEntity.Properties"/>.
			/// </summary>
			protected override ITableEntity CreateTableEntity(EventData data)
			{
				var tableEntity = new EventDataTableEntity<EventData>(data);
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

			#endregion
		}
	}
}