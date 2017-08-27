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
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Messages;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.BlobStorage.Events
{
	/// <summary>
	/// An Azure Storage based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class TableStorageEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		/// <summary>
		/// The pattern used to generate the stream name.
		/// </summary>
		protected const string TableCqrsEventStoreStreamNamePattern = "{0}.{1}";

		/// <summary>
		/// Gets or sets the underlying <see cref="TableStorageStore"/> used for persisting and reading <see cref="IEvent{TAuthenticationToken}"/> data.
		/// </summary>
		protected RawTableStorageEventStore TableStorageStore { get; set; }

		/// <summary>
		/// Gets or sets the underlying <see cref="TableStorageStore"/> used specifically for <see cref="Get(Guid)"/>.
		/// </summary>
		protected RawTableStorageEventStore CorrelationIdTableStorageStore { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageEventStore{TAuthenticationToken}"/> class using the specified container.
		/// </summary>
		public TableStorageEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory, Func<ILogger, ITableStorageStoreConnectionStringFactory, bool, RawTableStorageEventStore> createRawTableStorageEventStoreFunction = null)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			if (createRawTableStorageEventStoreFunction == null)
				createRawTableStorageEventStoreFunction = (logger1, tableStorageEventStoreConnectionStringFactory1, isCorrelationIdTableStorageStore) => new RawTableStorageEventStore(logger1, tableStorageEventStoreConnectionStringFactory1, isCorrelationIdTableStorageStore);
			TableStorageStore = createRawTableStorageEventStoreFunction(logger, tableStorageEventStoreConnectionStringFactory, false);
			CorrelationIdTableStorageStore = createRawTableStorageEventStoreFunction(logger, tableStorageEventStoreConnectionStringFactory, true);
		}

		#region Overrides of EventStore<TAuthenticationToken>

		/// <summary>
		/// Generate a unique stream name based on the provided <paramref name="aggregateRootType"/> and the <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		protected override string GenerateStreamName(Type aggregateRootType, Guid aggregateId)
		{
			return string.Format(TableCqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
		}

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

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
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

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override void PersistEvent(EventData eventData)
		{
			Logger.LogDebug("Adding data to the table storage event-store aggregate folder", "TableStorageStore\\Add");
			TableStorageStore.Add(eventData);
			Logger.LogDebug("Adding data to the table storage event-store by-correlation folder", "TableStorageStore\\Add");
			CorrelationIdTableStorageStore.Add(eventData);
		}

		#endregion

		/// <summary>
		/// An Azure Storage based <see cref="Cqrs.Azure.BlobStorage.TableStorageStore{TData,TCollectionItemData}"/>.
		/// </summary>
		public class RawTableStorageEventStore
			: TableStorageStore<EventDataTableEntity<EventData>, EventData>
		{
			private string TableName { get; set; }

			/// <summary>
			/// Indicates if this is a <see cref="Cqrs.Azure.BlobStorage.TableStorageStore{TData,TCollectionItemData}"/>
			/// for <see cref="IEventStore{TAuthenticationToken}.Get(Guid)"/>
			/// </summary>
			protected bool IsCorrelationIdTableStorageStore { get; set; }

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

			/// <summary>
			/// Returns <see cref="TableName"/>.
			/// </summary>
			/// <param name="sourceName">Is not used.</param>
			/// <returns><see cref="TableName"/></returns>
			protected override string GetSafeSourceName(string sourceName)
			{
				return TableName;
			}

			#endregion

			#region Overrides of TableStorageStore<EventData>

			/// <summary>
			/// Creates a new <see cref="EventDataTableEntity{TEventData}"/>.
			/// </summary>
			protected override ITableEntity CreateTableEntity(EventData data)
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

			/// <summary>
			/// Will throw an <see cref="InvalidOperationException"/> as this is not supported.
			/// </summary>
			protected override TableOperation GetUpdatableTableEntity(EventData data)
			{
				throw new InvalidOperationException("Event store entries are not updateable.");
			}

			/// <summary>
			/// Will throw an <see cref="InvalidOperationException"/> as this is not supported.
			/// </summary>
			protected override TableOperation GetUpdatableTableEntity(EventDataTableEntity<EventData> data)
			{
				return GetUpdatableTableEntity(data.EventData);
			}

			#endregion
		}
	}
}