#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Events;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.BlobStorage.Events
{
	public class TableStorageEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		protected RawTableStorageEventStore TableStorageStore { get; private set; }

		protected RawTableStorageEventStore CorrelationIdTableStorageStore { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageEventStore{TAuthenticationToken}"/> class using the specified container.
		/// </summary>
		public TableStorageEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			TableStorageStore = new RawTableStorageEventStore(logger, tableStorageEventStoreConnectionStringFactory);
			CorrelationIdTableStorageStore = new RawTableStorageEventStore(logger, tableStorageEventStoreConnectionStringFactory, true);
		}

		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			// Create the table query.
			var rangeQuery = new TableQuery<EventDataTableEntity<EventData>>().Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(streamName))
			);

			IEnumerable<EventData> query = TableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
				.Select(eventData => eventData.EventData)
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
			var rangeQuery = new TableQuery<EventDataTableEntity<EventData>>().Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(correlationId.ToString("N")))
			);

			IEnumerable<EventData> query = CorrelationIdTableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
				.Select(eventData => eventData.EventData)
				.OrderBy(eventData => eventData.Timestamp);

				return query.ToList();
		}

		protected override void PersistEvent(EventData eventData)
		{
			Logger.LogDebug("Adding data to the table storage event-store aggregate folder", "TableStorageStore\\Add");
			TableStorageStore.Add(eventData);
			Logger.LogDebug("Adding data to the table storage event-store by-correlation folder", "TableStorageStore\\Add");
			CorrelationIdTableStorageStore.Add(eventData);
		}

		#endregion

		public class RawTableStorageEventStore
			: TableStorageStore<EventDataTableEntity<EventData>, EventData>
		{
			private string TableName { get; set; }

			private bool IsCorrelationIdTableStorageStore { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="RawTableStorageEventStore"/> class using the specified container.
			/// </summary>
			public RawTableStorageEventStore(ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory, bool isCorrelationIdTableStorageStore = false)
				: base(logger)
			{
				GetContainerName = tableStorageEventStoreConnectionStringFactory.GetBaseContainerName;
				IsContainerPublic = () => false;

				IsCorrelationIdTableStorageStore = isCorrelationIdTableStorageStore;
				TableName = IsCorrelationIdTableStorageStore ? "EventStoreByCorrelationId" : "EventStore";

				// ReSharper disable DoNotCallOverridableMethodsInConstructor
				Initialise(tableStorageEventStoreConnectionStringFactory);
				// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}

			#region Overrides of StorageStore<EventData,CloudTable>

			protected override string GetSafeSourceName(string sourceName)
			{
				return TableName;
			}

			#endregion

			#region Overrides of TableStorageStore<EventData>

			protected override TableEntity CreateTableEntity(EventData data)
			{
				return new EventDataTableEntity<EventData>(data, IsCorrelationIdTableStorageStore);
			}

			/// <summary>
			/// Will mark the <paramref name="data"/> as logically (or soft).
			/// </summary>
			public override void Remove(EventData data)
			{
				throw new InvalidOperationException("Event store entries are not deletable.");
			}

			protected override TableOperation GetUpdatableTableEntity(EventData data)
			{
				throw new InvalidOperationException("Event store entries are not updatable.");
			}

			protected override TableOperation GetUpdatableTableEntity(EventDataTableEntity<EventData> data)
			{
				return GetUpdatableTableEntity(data.EventData);
			}

			#endregion
		}
	}
}