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
using Chinchilla.Logging;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Azure.BlobStorage.Events
{
	/// <summary>
	/// An Azure blob storage based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class BlobStorageEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		/// <summary>
		/// Get the <see cref="RawBlobStorageEventStore"/>.
		/// </summary>
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

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
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

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToVersion(Type aggregateRootType, Guid aggregateId, int version)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<EventData> query = BlobStorageStore
				.GetByFolder(streamName)
				.Where(eventData => eventData.AggregateId == streamName && eventData.Version <= version)
				.OrderByDescending(eventData => eventData.Version);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToDate(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<EventData> query = BlobStorageStore
				.GetByFolder(streamName)
				.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp <= versionedDate)
				.OrderByDescending(eventData => eventData.Version);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate,
			DateTime toVersionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<EventData> query = BlobStorageStore
				.GetByFolder(streamName)
				.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp >= fromVersionedDate && eventData.Timestamp <= toVersionedDate)
				.OrderByDescending(eventData => eventData.Version);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			IEnumerable<EventData> query = BlobStorageStore
				.GetByFolder(string.Format("..\\by-correlation\\{0:N}", correlationId))
				.Where(eventData => eventData.CorrelationId == correlationId)
				.OrderBy(eventData => eventData.Timestamp);

				return query.ToList();
		}

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override void PersistEvent(EventData eventData)
		{
			Logger.LogDebug("Adding data to the blob storage event-store aggregate folder", "BlobStorageStore\\Add");
			BlobStorageStore.Add(eventData);
			Logger.LogDebug("Adding data to the blob storage event-store by-correlation folder", "BlobStorageStore\\Add");
			BlobStorageStore.AddToCorrelationFolder(eventData);
		}

		#endregion

		/// <summary>
		/// The raw <see cref="Cqrs.Azure.BlobStorage.BlobStorageStore{TEventData}"/>.
		/// </summary>
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

			/// <summary>
			/// Add the provided <paramref name="data"/> into the correlation folder.
			/// </summary>
			public virtual void AddToCorrelationFolder(EventData data)
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