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

namespace Cqrs.Azure.DocumentDb
{
	public class AzureDocumentDbEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken>
	{
		protected IEventStoreConnectionHelper EventStoreConnectionHelper { get; set; }

		public AzureDocumentDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, IEventStoreConnectionHelper eventStoreConnectionHelper)
			: base(eventBuilder, eventDeserialiser)
		{
			EventStoreConnectionHelper = eventStoreConnectionHelper;
		}

		public override IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return GetAsync<T>(aggregateId, useLastEventOnly, fromVersion).Result;
		}

		protected async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			using (DocumentClient client = EventStoreConnectionHelper.GetEventStoreConnection())
			{
				var database = new Database { Id = EventStoreConnectionHelper.GetEventStoreConnectionLogStreamName() };
				database = await client.CreateDatabaseAsync(database);

				var collection = new DocumentCollection { Id = "CqrsEventStore" };
				collection = await client.CreateDocumentCollectionAsync(database.SelfLink, collection);

				var query = client.CreateDocumentQuery<EventData>(collection.SelfLink);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, typeof(T).FullName, aggregateId);

				IOrderedQueryable<EventData> results = query.Where(x => x.AggregateId == streamName)
					.OrderByDescending(x => x.EventId);

				return results.Select(EventDeserialiser.Deserialise);
			}
		}

		protected override async void PersitEvent(EventData eventData)
		{
			using (DocumentClient client = EventStoreConnectionHelper.GetEventStoreConnection())
			{
				var database = new Database {Id = EventStoreConnectionHelper.GetEventStoreConnectionLogStreamName()};
				database = await client.CreateDatabaseAsync(database);

				var collection = new DocumentCollection {Id =  "CqrsEventStore"};
				collection = await client.CreateDocumentCollectionAsync(database.SelfLink, collection);

				await client.CreateDocumentAsync(collection.SelfLink, eventData);
			}
		}
	}
}