#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Events;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Cqrs.Azure.BlobStorage.Events
{
	public class BlobStorageEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		protected RawBlobStorageEventStore BlobStorageStore { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
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
			/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
			/// </summary>
			public RawBlobStorageEventStore(ILogger logger, IBlobStorageStoreConnectionStringFactory blobStorageEventStoreConnectionStringFactory)
				: base(logger)
			{
				GetContainerName = blobStorageEventStoreConnectionStringFactory.GetBaseContainerName;
				IsContainerPublic = () => false;
				GenerateFileName = data => Path.Combine(data.AggregateId, string.Format("{0}-{1}",data.Version, data.EventId.ToString("N")));

				// ReSharper disable DoNotCallOverridableMethodsInConstructor
				Initialise(blobStorageEventStoreConnectionStringFactory);
				// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}

			public void AddToCorrelationFolder(EventData data)
			{
				foreach (Tuple<CloudStorageAccount, CloudBlobContainer> tuple in WritableCollection)
				{
					CloudBlockBlob cloudBlockBlob = GetBlobReference(tuple.Item2, string.Format("by-correlation\\{0}\\{1}", data.CorrelationId, GenerateFileName(data)));
					Uri uri = AzureStorageRetryPolicy.ExecuteAction
					(
						() =>
						{
							cloudBlockBlob.UploadFromStream(Serialise(data));
							cloudBlockBlob.Properties.ContentType = "application/json";
							cloudBlockBlob.SetProperties();
							return cloudBlockBlob.Uri;
						}
					);

					Logger.LogDebug(string.Format("The data entity '{0}' was persisted at uri '{1}'", GenerateFileName(data), uri));
				}
			}
		}
	}
}