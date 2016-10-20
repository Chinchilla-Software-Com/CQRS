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
using cdmdotnet.Logging;
using Cqrs.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A MongoDB based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	public class MongoDbEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken> 
	{
		protected IMongoCollection<EventData> MongoCollection { get; private set; }

		protected IMongoDbEventStoreConnectionStringFactory MongoDbEventStoreConnectionStringFactory { get; private set; }

		public MongoDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IMongoDbEventStoreConnectionStringFactory mongoDbEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			MongoDbEventStoreConnectionStringFactory = mongoDbEventStoreConnectionStringFactory;

			MongoCollection = GetCollection();
		}

		protected virtual IMongoCollection<EventData> GetCollection()
		{
			var mongoClient = new MongoClient(MongoDbEventStoreConnectionStringFactory.GetEventStoreConnectionString());
			IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoDbEventStoreConnectionStringFactory.GetEventStoreDatabaseName());

			return mongoDatabase.GetCollection<EventData>(MongoDbEventStoreConnectionStringFactory.GetEventStoreDatabaseName());
		}


		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<EventData> query = MongoCollection
				.AsQueryable()
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
			IEnumerable<EventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.CorrelationId == correlationId)
				.OrderBy(eventData => eventData.Timestamp);

			return query.ToList();
		}

		protected override void PersistEvent(EventData eventData)
		{
			Logger.LogDebug("Adding an event to the MongoDB database", "MongoDbEventStore\\PersistEvent");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.InsertOne(eventData);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the MongoDB database took {0}.", end - start), "MongoDbEventStore\\PersistEvent");
			}
			finally
			{
				Logger.LogDebug("Adding an event to the MongoDB database... Done", "MongoDbEventStore\\PersistEvent");
			}
		}

		#endregion
	}
}