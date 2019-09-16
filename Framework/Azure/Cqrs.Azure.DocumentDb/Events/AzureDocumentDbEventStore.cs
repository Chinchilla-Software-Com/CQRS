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
using System.Threading.Tasks;
using Cqrs.Events;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Messages;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.Events
{
	/// <summary>
	/// A DocumentDb based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureDocumentDbEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		/// <summary>
		/// The properties that must be unique.
		/// </summary>
		protected readonly string[] UniqueIndexProperties = {"AggregateId", "Version"};

		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbEventStoreConnectionStringFactory"/>
		/// </summary>
		protected IAzureDocumentDbEventStoreConnectionStringFactory AzureDocumentDbEventStoreConnectionStringFactory { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbHelper"/>
		/// </summary>
		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="AzureDocumentDbEventStore{TAuthenticationToken}"/>.
		/// </summary>
		public AzureDocumentDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IAzureDocumentDbHelper azureDocumentDbHelper, IAzureDocumentDbEventStoreConnectionStringFactory azureDocumentDbEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			AzureDocumentDbHelper = azureDocumentDbHelper;
			AzureDocumentDbEventStoreConnectionStringFactory = azureDocumentDbEventStoreConnectionStringFactory;
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
			return GetAsync(aggregateRootType, aggregateId, useLastEventOnly, fromVersion).Result;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToVersion(Type aggregateRootType, Guid aggregateId, int version)
		{
			return GetToVersionAsync(aggregateRootType, aggregateId, version).Result;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="System.DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToDate(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			return GetToDateAsync(aggregateRootType, aggregateId, versionedDate).Result;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="System.DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="System.DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			return GetBetweenDatesAsync(aggregateRootType, aggregateId, fromVersionedDate, toVersionedDate).Result;
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			return GetAsync(correlationId).Result;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <typeparamref name="T"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <typeparam name="T"><see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		/// <returns></returns>
		protected async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"><see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		protected async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionClient())
			{
				Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionDatabaseName()).Result;
				//DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, string.Format("{0}_{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), typeof(T).FullName)).Result;
				string collectionName = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName();
				DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

				IEnumerable<EventData> results = query.Where(x => x.AggregateId == streamName && x.Version > fromVersion);

				return AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() =>
					results
						.ToList()
						.OrderByDescending(x => x.EventId)
						.Select(EventDeserialiser.Deserialise)
				);
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToVersionAsync(Type aggregateRootType, Guid aggregateId, int version)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionClient())
			{
				Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionDatabaseName()).Result;
				//DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, string.Format("{0}_{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), typeof(T).FullName)).Result;
				string collectionName = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName();
				DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

				IEnumerable<EventData> results = query.Where(x => x.AggregateId == streamName && x.Version <= version);

				return AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() =>
					results
						.ToList()
						.OrderByDescending(x => x.EventId)
						.Select(EventDeserialiser.Deserialise)
				);
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="System.DateTime"/></param>
		public async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToDateAsync(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionClient())
			{
				Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionDatabaseName()).Result;
				//DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, string.Format("{0}_{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), typeof(T).FullName)).Result;
				string collectionName = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName();
				DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

				IEnumerable<EventData> results = query.Where(x => x.AggregateId == streamName && x.Timestamp <= versionedDate);

				return AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() =>
					results
						.ToList()
						.OrderByDescending(x => x.EventId)
						.Select(EventDeserialiser.Deserialise)
				);
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="System.DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="System.DateTime"/></param>
		public async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetBetweenDatesAsync(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionClient())
			{
				Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionDatabaseName()).Result;
				//DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, string.Format("{0}_{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), typeof(T).FullName)).Result;
				string collectionName = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName();
				DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

				IEnumerable<EventData> results = query.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp >= fromVersionedDate && eventData.Timestamp <= toVersionedDate);

				return AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() =>
					results
						.ToList()
						.OrderByDescending(x => x.EventId)
						.Select(EventDeserialiser.Deserialise)
				);
			}
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		protected async Task<IEnumerable<EventData>> GetAsync(Guid correlationId)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionClient())
			{
				Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionDatabaseName()).Result;
				//DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, string.Format("{0}_{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), typeof(T).FullName)).Result;
				string collectionName = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName();
				DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);

				IEnumerable<EventData> results = query.Where(x => x.CorrelationId == correlationId);

				return AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() =>
					results
						.ToList()
						.OrderBy(x => x.Timestamp)
				);
			}
		}

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override void PersistEvent(EventData eventData)
		{
			Logger.LogDebug("Persisting aggregate root event", string.Format("{0}\\PersitEvent", GetType().Name));
			try
			{
				using (DocumentClient client = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionClient())
				{
					Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionDatabaseName()).Result;
					//DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, string.Format("{0}_{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), eventData.EventType)).Result;
					//string collectionName = string.Format("{0}::{1}", AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName(), eventData.AggregateId.Substring(0, eventData.AggregateId.IndexOf("/", StringComparison.Ordinal)));
					string collectionName = AzureDocumentDbEventStoreConnectionStringFactory.GetEventStoreConnectionCollectionName();
					DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

					Logger.LogDebug("Creating document for event asynchronously", string.Format("{0}\\PersitEvent", GetType().Name));
					AzureDocumentDbHelper.ExecuteFaultTollerantFunction
					(
						() =>
						{
							Task<ResourceResponse<Document>> work = client.CreateDocumentAsync
							(
								collection.SelfLink,
								eventData
							);
							work.ConfigureAwait(false);
							work.Wait();
						}
					);
				}
			}
			finally
			{
				Logger.LogDebug("Persisting aggregate root event... Done", string.Format("{0}\\PersitEvent", GetType().Name));
			}
		}
	}
}