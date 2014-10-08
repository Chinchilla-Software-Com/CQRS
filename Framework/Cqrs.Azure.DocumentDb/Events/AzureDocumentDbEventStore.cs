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
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.Events
{
	public class AzureDocumentDbEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken>
	{
		protected IAzureDocumentDbEventStoreConnectionHelper AzureDocumentDbEventStoreConnectionHelper { get; set; }

		public AzureDocumentDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, IAzureDocumentDbEventStoreConnectionHelper azureDocumentDbEventStoreConnectionHelper)
			: base(eventBuilder, eventDeserialiser)
		{
			AzureDocumentDbEventStoreConnectionHelper = azureDocumentDbEventStoreConnectionHelper;
		}

		public override IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return GetAsync<T>(aggregateId, useLastEventOnly, fromVersion).Result;
		}

		protected async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionHelper.GetEventStoreConnection())
			{
				Database database = CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionHelper.GetEventStoreConnectionLogStreamName()).Result;
				DocumentCollection collection = CreateOrReadCollection(client, database, "CqrsEventStore").Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, typeof(T).FullName, aggregateId);

				IEnumerable<EventData> results = query.Where(x => x.AggregateId == streamName);

				return results
					.ToList()
					.OrderByDescending(x => x.EventId)
					.Select(EventDeserialiser.Deserialise);
			}
		}

		protected override async void PersitEvent(EventData eventData)
		{
			using (DocumentClient client = AzureDocumentDbEventStoreConnectionHelper.GetEventStoreConnection())
			{
				Database database = CreateOrReadDatabase(client, AzureDocumentDbEventStoreConnectionHelper.GetEventStoreConnectionLogStreamName()).Result;
				DocumentCollection collection = CreateOrReadCollection(client, database, "CqrsEventStore").Result;

				await client.CreateDocumentAsync(collection.SelfLink, eventData);
			}
		}

		protected async Task<Database> CreateOrReadDatabase(DocumentClient client, string databaseName)
		{
			IEnumerable<Database> query = client.CreateDatabaseQuery()
				.Where(database => database.Id == databaseName)
				.AsEnumerable();
			Database result = query.SingleOrDefault();
			return result ?? await client.CreateDatabaseAsync(new Database { Id = databaseName });
		}

		protected async Task<DocumentCollection> CreateOrReadCollection(DocumentClient client, Database database, string collectionName)
		{
			IEnumerable<DocumentCollection> query = client.CreateDocumentCollectionQuery(database.SelfLink)
				.Where(documentCollection => documentCollection.Id == collectionName)
				.AsEnumerable();
			DocumentCollection result = query.SingleOrDefault();
			return result ?? await client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collectionName });
		}
	}
}