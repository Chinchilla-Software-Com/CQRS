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
using System.Threading.Tasks;
using Cqrs.Events;
using cdmdotnet.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.Events
{
	public class AzureDocumentDbEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken>
	{
		protected readonly string[] UniqueIndexProperties = {"AggregateId", "Version"};

		protected IAzureDocumentDbEventStoreConnectionStringFactory AzureDocumentDbEventStoreConnectionStringFactory { get; private set; }

		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		public AzureDocumentDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IAzureDocumentDbHelper azureDocumentDbHelper, IAzureDocumentDbEventStoreConnectionStringFactory azureDocumentDbEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			AzureDocumentDbHelper = azureDocumentDbHelper;
			AzureDocumentDbEventStoreConnectionStringFactory = azureDocumentDbEventStoreConnectionStringFactory;
		}

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return GetAsync(aggregateRootType, aggregateId, useLastEventOnly, fromVersion).Result;
		}

		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			return GetAsync(correlationId).Result;
		}

		protected async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

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
								eventData,
								new RequestOptions {PreTriggerInclude = new[] {"ValidateUniqueConstraints"}}
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
