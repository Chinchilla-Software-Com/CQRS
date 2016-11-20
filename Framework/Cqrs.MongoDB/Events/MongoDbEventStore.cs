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
using System.Linq.Expressions;
using System.Reflection;
using cdmdotnet.Logging;
using Cqrs.Events;
using Cqrs.MongoDB.DataStores.Indexes;
using Cqrs.MongoDB.Events.Indexes;
using Cqrs.MongoDB.Factories;
using Cqrs.MongoDB.Serialisers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A MongoDB based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	public class MongoDbEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken> 
	{
		protected IMongoCollection<MongoDbEventData> MongoCollection { get; private set; }

		protected IMongoDbEventStoreConnectionStringFactory MongoDbEventStoreConnectionStringFactory { get; private set; }

		static MongoDbEventStore()
		{
			IDictionary<Type, IList<object>> randomCallToStartStaticProperty = MongoDbDataStoreFactory.IndexTypesByEntityType;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var eventTypes = assembly
					.GetTypes()
					.Where
					(
						type =>
							typeof(IEvent<TAuthenticationToken>).IsAssignableFrom(type)
							&& !type.IsAbstract
					);
				foreach (Type eventType in eventTypes)
					BsonClassMap.LookupClassMap(eventType);
			}
		}

		public MongoDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IMongoDbEventStoreConnectionStringFactory mongoDbEventStoreConnectionStringFactory)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			MongoDbEventStoreConnectionStringFactory = mongoDbEventStoreConnectionStringFactory;

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			MongoCollection = GetCollection();
			VerifyIndexes();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		protected virtual IMongoCollection<MongoDbEventData> GetCollection()
		{
			var mongoClient = new MongoClient(MongoDbEventStoreConnectionStringFactory.GetEventStoreConnectionString());
			IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoDbEventStoreConnectionStringFactory.GetEventStoreDatabaseName());

			return mongoDatabase.GetCollection<MongoDbEventData>(MongoDbEventStoreConnectionStringFactory.GetEventStoreDatabaseName());
		}

		protected virtual void VerifyIndexes()
		{
			VerifyIndex(new ByCorrelationIdMongoDbIndex());
			VerifyIndex(new ByAggregateIdAndVersionMongoDbIndex());
			VerifyIndex(new ByTimestampMongoDbIndex());
			VerifyIndex(new ByTimestampAndEventTypeMongoDbIndex());
		}

		protected virtual void VerifyIndex(MongoDbIndex<MongoDbEventData> mongoIndex)
		{
			IndexKeysDefinitionBuilder<MongoDbEventData> indexKeysBuilder = Builders<MongoDbEventData>.IndexKeys;
			IndexKeysDefinition<MongoDbEventData> indexKey = null;

			IList<Expression<Func<MongoDbEventData, object>>> selectors = mongoIndex.Selectors.ToList();
			for (int i = 0; i < selectors.Count; i++)
			{
				Expression<Func<MongoDbEventData, object>> expression = selectors[i];
				if (mongoIndex.IsAcending)
				{
					if (i == 0)
						indexKey = indexKeysBuilder.Ascending(expression);
					else
						indexKey = indexKey.Ascending(expression);
				}
				else
				{
					if (i == 0)
						indexKey = indexKeysBuilder.Descending(expression);
					else
						indexKey = indexKey.Descending(expression);
				}
			}

			MongoCollection.Indexes.CreateOne
			(
				indexKey,
				new CreateIndexOptions
				{
					Unique = mongoIndex.IsUnique,
					Name = mongoIndex.Name
				}
			);
		}

		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<MongoDbEventData> query = MongoCollection
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
			IEnumerable<MongoDbEventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.CorrelationId == correlationId)
				.OrderBy(eventData => eventData.Timestamp);

			return query.ToList();
		}

		protected override void PersistEvent(EventData eventData)
		{
			var safeEventData = eventData as MongoDbEventData;
			if (safeEventData == null)
				safeEventData = new MongoDbEventData(eventData);
			Logger.LogDebug("Adding an event to the MongoDB database", "MongoDbEventStore\\PersistEvent");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.InsertOne(safeEventData);
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