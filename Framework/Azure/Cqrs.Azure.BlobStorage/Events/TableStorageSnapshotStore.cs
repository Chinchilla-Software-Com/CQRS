#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Snapshots;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.BlobStorage.Events
{
	/// <summary>
	/// An Azure Storage based <see cref="SnapshotStore"/>.
	/// </summary>
	public class TableStorageSnapshotStore
		: SnapshotStore
	{
		/// <summary>
		/// The pattern used to generate the stream name.
		/// </summary>
		protected const string TableCqrsSnapshotStoreStreamNamePattern = "{0}.{1}";

		/// <summary>
		/// Gets or sets the underlying <see cref="TableStorageStore"/> used for persisting and reading <see cref="Snapshot"/> data.
		/// </summary>
		protected RawTableStorageSnapshotStore TableStorageStore { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageEventStore{TAuthenticationToken}"/> class using the specified container.
		/// </summary>
		public TableStorageSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder, ITableStorageSnapshotStoreConnectionStringFactory tableStorageSnapshotStoreConnectionStringFactory, Func<ILogger, ITableStorageSnapshotStoreConnectionStringFactory, RawTableStorageSnapshotStore> createRawTableStorageSnapshotStoreFunction = null)
			: base(configurationManager, eventDeserialiser, snapshotBuilder, logger, correlationIdHelper)
		{
			if (createRawTableStorageSnapshotStoreFunction == null)
				createRawTableStorageSnapshotStoreFunction = (logger1, tableStorageSnapshotStoreConnectionStringFactory1) => new RawTableStorageSnapshotStore(logger1, tableStorageSnapshotStoreConnectionStringFactory1);
			TableStorageStore = createRawTableStorageSnapshotStoreFunction(logger, tableStorageSnapshotStoreConnectionStringFactory);
		}

		#region Overrides of SnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected override Snapshot Get(Type aggregateRootType, string streamName)
		{
			// Create the table query.
			var rangeQuery = new TableQuery<EventDataTableEntity<EventData>>().Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(streamName))
			);

			Snapshot result = TableStorageStore.ReadableSource.ExecuteQuery(rangeQuery)
				.Select(eventData => eventData.EventData)
				.Where(eventData => eventData.AggregateId == streamName)
				.OrderByDescending(eventData => eventData.Version)
				.Take(1)
				.Select(EventDeserialiser.Deserialise)
				.SingleOrDefault();

			return result;
		}

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshot"/> to save and store.</param>
		public override void Save(Snapshot snapshot)
		{
			Logger.LogDebug("Adding data to the table storage snapshot-store aggregate folder", "TableStorageStore\\Add");
			TableStorageStore.Add(BuildEventData(snapshot));
			Logger.LogDebug("Added data to the table storage snapshot-store aggregate folder", "TableStorageStore\\Save");
		}

		/// <summary>
		/// Generate a unique stream name based on the provided <paramref name="aggregateRootType"/> and the <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		protected override string GenerateStreamName(Type aggregateRootType, Guid aggregateId)
		{
			return string.Format(TableCqrsSnapshotStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
		}

		#endregion

		/// <summary>
		/// An Azure Storage based <see cref="Cqrs.Azure.BlobStorage.TableStorageStore{TData,TCollectionItemData}"/>.
		/// </summary>
		public class RawTableStorageSnapshotStore
			: TableStorageStore<EventDataTableEntity<EventData>, EventData>
		{
			private string TableName { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="RawTableStorageSnapshotStore"/> class using the specified container.
			/// </summary>
			public RawTableStorageSnapshotStore(ILogger logger, ITableStorageSnapshotStoreConnectionStringFactory tableStorageSnapshotStoreConnectionStringFactory)
				: base(logger)
			{
				GetContainerName = tableStorageSnapshotStoreConnectionStringFactory.GetBaseContainerName;
				IsContainerPublic = () => false;

				TableName = "SnapshotStore";

				// ReSharper disable DoNotCallOverridableMethodsInConstructor
				Initialise(tableStorageSnapshotStoreConnectionStringFactory);
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
				return new EventDataTableEntity<EventData>(data);
			}

			/// <summary>
			/// Will mark the <paramref name="data"/> as logically (or soft).
			/// </summary>
			public override void Remove(EventData data)
			{
				throw new InvalidOperationException("Snapshot store entries are not deletable.");
			}

			/// <summary>
			/// Will throw an <see cref="InvalidOperationException"/> as this is not supported.
			/// </summary>
			protected override TableOperation GetUpdatableTableEntity(EventData data)
			{
				throw new InvalidOperationException("Snapshot store entries are not updateable.");
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