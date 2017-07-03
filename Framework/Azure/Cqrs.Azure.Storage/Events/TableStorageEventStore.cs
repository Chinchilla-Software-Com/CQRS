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
using cdmdotnet.Logging;
using Cqrs.Azure.BlobStorage;
using Cqrs.Events;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.Storage.Events
{
	public class TableStorageEventStore<TAuthenticationToken>
		: BlobStorage.Events.TableStorageEventStore<TAuthenticationToken>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageEventStore{TAuthenticationToken}"/> class using the specified container.
		/// </summary>
		public TableStorageEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger, tableStorageEventStoreConnectionStringFactory, (logger1, tableStorageEventStoreConnectionStringFactory1, isCorrelationIdTableStorageStore) => new RawTableStorageEventStore(logger1, tableStorageEventStoreConnectionStringFactory1, isCorrelationIdTableStorageStore))
		{
		}

		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = GenerateStreamName(aggregateRootType, aggregateId);

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
			IEnumerable<EventData> query = TableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
				.Select(eventData => EntityPropertyConverter.ConvertBack<EventData>(eventData.Properties, operationContext))
				.Where(eventData => eventData.AggregateId == streamName && eventData.Version > fromVersion)
				.OrderByDescending(eventData => eventData.Version);

			if (useLastEventOnly)
				query = query.AsQueryable().Take(1);

			return query
				.Select(eventData => EventDeserialiser.Deserialise(eventData))
				.ToList();
		}

		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			// Create the table query.
			var rangeQuery = new TableQuery<DynamicTableEntity>().Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(correlationId.ToString("N")))
			);

			var operationContext = new OperationContext();
			IEnumerable<EventData> query = CorrelationIdTableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
				.Select(eventData => EntityPropertyConverter.ConvertBack<EventData>(eventData.Properties, operationContext))
				.OrderBy(eventData => eventData.Timestamp);

				return query.ToList();
		}

		#endregion

		public class RawTableStorageEventStore
			: BlobStorage.Events.TableStorageEventStore<TAuthenticationToken>.RawTableStorageEventStore
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="RawTableStorageEventStore"/> class using the specified container.
			/// </summary>
			public RawTableStorageEventStore(ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory, bool isCorrelationIdTableStorageStore = false)
				: base(logger, tableStorageEventStoreConnectionStringFactory, isCorrelationIdTableStorageStore)
			{
			}

			#region Overrides of StorageStore<EventData,CloudTable>

			protected override string GetSafeSourceName(string sourceName)
			{
				string tableName = base.GetSafeSourceName(sourceName);
				if (tableName.Length > 34)
					tableName = tableName.Substring(tableName.Length - 34);
				return string.Format("{0}V2", tableName);
			}

			#endregion

			#region Overrides of TableStorageStore<EventData>

			protected override ITableEntity CreateTableEntity(EventData data)
			{
				var tableEntity = new EventDataTableEntity<EventData>(data, IsCorrelationIdTableStorageStore);
				//Flatten object of type TData and convert it to EntityProperty Dictionary
				Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(data, new OperationContext());

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