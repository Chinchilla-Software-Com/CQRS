#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Events;

namespace Cqrs.Azure.BlobStorage.Events
{
	public class BlobStorageEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		protected RawBlobStorageEventStore BlobStorageStore { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageEventStore{TAuthenticationToken}"/> class using the specified container.
		/// </summary>
		public BlobStorageEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IBlobStorageStoreConnectionStringFactory blobStorageEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			BlobStorageStore = new RawBlobStorageEventStore(logger, blobStorageEventStoreConnectionStringFactory);
		}

		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<EventData> query = BlobStorageStore
				.GetByFolder(streamName)
				.Where(eventData => eventData.AggregateId == streamName && eventData.Version > fromVersion)
				.OrderByDescending(eventData => eventData.Version);

			if (useLastEventOnly)
				query = query.AsQueryable().Take(1);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			IEnumerable<EventData> query = BlobStorageStore
				.GetByFolder(string.Format("..\\by-correlation\\{0:N}", correlationId))
				.Where(eventData => eventData.CorrelationId == correlationId)
				.OrderBy(eventData => eventData.Timestamp);

				return query.ToList();
		}

		protected override void PersistEvent(EventData eventData)
		{
			Logger.LogDebug("Adding data to the blob storage event-store aggregate folder", "BlobStorageStore\\Add");
			BlobStorageStore.Add(eventData);
			Logger.LogDebug("Adding data to the blob storage event-store by-correlation folder", "BlobStorageStore\\Add");
			BlobStorageStore.AddToCorrelationFolder(eventData);
		}

		#endregion

		public class RawBlobStorageEventStore
			: BlobStorageStore<EventData>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="RawBlobStorageEventStore"/> class using the specified container.
			/// </summary>
			public RawBlobStorageEventStore(ILogger logger, IBlobStorageStoreConnectionStringFactory blobStorageEventStoreConnectionStringFactory)
				: base(logger)
			{
				GetContainerName = blobStorageEventStoreConnectionStringFactory.GetBaseContainerName;
				IsContainerPublic = () => false;
				GenerateFileName = data => Path.Combine(data.AggregateId, string.Format("{0:D10}\\{1}",data.Version, data.EventId.ToString("N")));

				// ReSharper disable DoNotCallOverridableMethodsInConstructor
				Initialise(blobStorageEventStoreConnectionStringFactory);
				// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}

			public void AddToCorrelationFolder(EventData data)
			{
				AsyncSaveData
				(
					data,
					(taskData, cloudBlockBlob) =>
					{
						try
						{
							cloudBlockBlob.UploadFromStream(Serialise(taskData));
							cloudBlockBlob.Properties.ContentType = "application/json";
							cloudBlockBlob.SetProperties();
							return cloudBlockBlob.Uri;
						}
						catch (Exception exception)
						{
							Logger.LogError("There was an issue persisting data to blob storage.", exception: exception);
							throw;
						}
					},
					taskData => string.Format("by-correlation\\{0:N}\\{1}", data.CorrelationId, GenerateFileName(data))
				);
			}
		}
	}
}