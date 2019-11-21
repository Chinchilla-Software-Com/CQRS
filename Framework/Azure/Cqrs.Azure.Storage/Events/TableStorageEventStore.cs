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
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Messages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.Storage.Events
{
	/// <summary>
	/// An Azure Storage based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class TableStorageEventStore<TAuthenticationToken>
		: BlobStorage.Events.TableStorageEventStore<TAuthenticationToken>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageEventStore{TAuthenticationToken}"/> class using the specified container.
		/// </summary>
		public TableStorageEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger, tableStorageEventStoreConnectionStringFactory, (logger1, tableStorageEventStoreConnectionStringFactory1, isCorrelationIdTableStorageStore) => new RawTableStorageEventStorer(logger1, tableStorageEventStoreConnectionStringFactory1, isCorrelationIdTableStorageStore))
		{
		}

		#region Overrides of EventStore<TAuthenticationToken>

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
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
#pragma warning disable 0436
				.Select(eventData => EntityPropertyConverter.ConvertBack<EventData>(eventData.Properties, operationContext))
#pragma warning restore 0436
				.Where(eventData => eventData.AggregateId == streamName && eventData.Version > fromVersion)
				.OrderByDescending(eventData => eventData.Version);

			if (useLastEventOnly)
				query = query.AsQueryable().Take(1);

			return query
				.Select(eventData => EventDeserialiser.Deserialise(eventData))
				.ToList();
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			// Create the table query.
			var rangeQuery = new TableQuery<DynamicTableEntity>().Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(correlationId.ToString("N")))
			);

			var operationContext = new OperationContext();
			IEnumerable<EventData> query = CorrelationIdTableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
#pragma warning disable 0436
				.Select(eventData => EntityPropertyConverter.ConvertBack<EventData>(eventData.Properties, operationContext))
#pragma warning restore 0436
				.OrderBy(eventData => eventData.Timestamp);

				return query.ToList();
		}

		#endregion

		/// <summary>
		/// An Azure Storage based <see cref="TableStorageStore{TData,TCollectionItemData}"/>.
		/// </summary>
		public class RawTableStorageEventStorer
			: RawTableStorageEventStore
		{
			/// <summary>
			/// Initializes a new instance of the RawTableStorageEventStorer class using the specified container.
			/// </summary>
			public RawTableStorageEventStorer(ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory, bool isCorrelationIdTableStorageStore = false)
				: base(logger, tableStorageEventStoreConnectionStringFactory, isCorrelationIdTableStorageStore)
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
				var tableEntity = new EventDataTableEntity<EventData>(data, IsCorrelationIdTableStorageStore);
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