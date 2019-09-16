#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Snapshots;

namespace Cqrs.Azure.BlobStorage.Events
{
	/// <summary>
	/// An Azure blob storage based <see cref="SnapshotStore"/>.
	/// </summary>
	public class BlobStorageSnapshotStore
		: SnapshotStore
	{
		/// <summary>
		/// Get the <see cref="BlobStorageStore"/>.
		/// </summary>
		protected BlobStorageStore<EventData> BlobStorageStore { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageSnapshotStore"/> class using the specified container.
		/// </summary>
		public BlobStorageSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder, IBlobStorageSnapshotStoreConnectionStringFactory blobStorageSnapshotStoreConnectionStringFactory)
			: base(configurationManager, eventDeserialiser, snapshotBuilder, logger, correlationIdHelper)
		{
			BlobStorageStore = new RawBlobStorageSnapshotStore(logger, blobStorageSnapshotStoreConnectionStringFactory);
		}

		#region Overrides of SnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected override Snapshot Get(Type aggregateRootType, string streamName)
		{
			Snapshot result = BlobStorageStore
				.GetByFolder(streamName)
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
			Logger.LogDebug("Adding data to the blob storage snapshot-store aggregate folder", "BlobStorageSnapshotStore\\Save");
			BlobStorageStore.Add(BuildEventData(snapshot));
			Logger.LogDebug("Added data to the blob storage snapshot-store aggregate folder", "BlobStorageSnapshotStore\\Save");
		}

		#endregion

		/// <summary>
		/// The raw <see cref="Cqrs.Azure.BlobStorage.BlobStorageStore{TEventData}"/>.
		/// </summary>
		public class RawBlobStorageSnapshotStore
			: BlobStorageStore<EventData>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="RawBlobStorageSnapshotStore"/> class using the specified container.
			/// </summary>
			public RawBlobStorageSnapshotStore(ILogger logger, IBlobStorageSnapshotStoreConnectionStringFactory blobStorageSnapshotStoreConnectionStringFactory)
				: base(logger)
			{
				GetContainerName = blobStorageSnapshotStoreConnectionStringFactory.GetBaseContainerName;
				IsContainerPublic = () => false;
				GenerateFileName = data => Path.Combine(data.AggregateId, string.Format("{0:D10}\\{1}",data.Version, data.EventId.ToString("N")));

				// ReSharper disable DoNotCallOverridableMethodsInConstructor
				Initialise(blobStorageSnapshotStoreConnectionStringFactory);
				// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}
		}
	}
}