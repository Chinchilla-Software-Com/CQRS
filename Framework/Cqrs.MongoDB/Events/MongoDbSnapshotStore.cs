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
using System.Linq.Expressions;
using System.Reflection;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.MongoDB.DataStores.Indexes;
using Cqrs.MongoDB.Events.Indexes;
using Cqrs.MongoDB.Factories;
using Cqrs.Snapshots;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// Stores the most recent <see cref="Snapshot">snapshots</see> for replay and <see cref="IAggregateRoot{TAuthenticationToken}"/> rehydration on a <see cref="SnapshotAggregateRoot{TAuthenticationToken,TSnapshot}"/> in MongoDB.
	/// </summary>
	public class MongoDbSnapshotStore
		: SnapshotStore
	{
		/// <summary>
		/// Gets or sets the <see cref="IMongoCollection{TData}"/>
		/// </summary>
		protected IMongoCollection<MongoDbEventData> MongoCollection { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IMongoDbEventStoreConnectionStringFactory"/>
		/// </summary>
		protected IMongoDbSnapshotStoreConnectionStringFactory MongoDbSnapshotStoreConnectionStringFactory { get; private set; }

		static MongoDbSnapshotStore()
		{
			IDictionary<Type, IList<object>> randomCallToStartStaticProperty = MongoDbDataStoreFactory.IndexTypesByEntityType;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var eventTypes = assembly
					.GetTypes()
					.Where
					(
						type =>
							typeof(EventData).IsAssignableFrom(type)
							&& !type.IsAbstract
					);
				foreach (Type eventType in eventTypes)
					BsonClassMap.LookupClassMap(eventType);
			}
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MongoDbEventStore{TAuthenticationToken}"/>
		/// triggering any require index checks.
		/// </summary>
		public MongoDbSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder, IMongoDbSnapshotStoreConnectionStringFactory mongoDbSnapshotStoreConnectionStringFactory)
			: base(configurationManager, eventDeserialiser, snapshotBuilder, logger, correlationIdHelper)
		{
			MongoDbSnapshotStoreConnectionStringFactory = mongoDbSnapshotStoreConnectionStringFactory;

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			MongoCollection = GetCollection();
			VerifyIndexes();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		/// <summary>
		/// Get a <see cref="IMongoCollection{TDocument}"/>
		/// </summary>
		protected virtual IMongoCollection<MongoDbEventData> GetCollection()
		{
			var mongoClient = new MongoClient(MongoDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionString());
			IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreDatabaseName());

			return mongoDatabase.GetCollection<MongoDbEventData>(MongoDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreDatabaseName());
		}

		/// <summary>
		/// Verify all required <see cref="MongoDbIndex{TEntity}"/> are defined and ready to go.
		/// </summary>
		protected virtual void VerifyIndexes()
		{
			VerifyIndex(new ByCorrelationIdMongoDbIndex());
			VerifyIndex(new ByAggregateIdAndVersionMongoDbIndex());
			VerifyIndex(new ByTimestampMongoDbIndex());
			VerifyIndex(new ByTimestampAndEventTypeMongoDbIndex());
		}

		/// <summary>
		/// Verify the provided <paramref name="mongoIndex"/> is defined and ready to go.
		/// </summary>
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

			bool throwExceptions;
			if (!bool.TryParse(ConfigurationManager.GetSetting("Cqrs.MongoDb.SnapshotStore.ThrowExceptionsOnIndexPreparation"), out throwExceptions))
				throwExceptions = true;
			try
			{
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
			catch
			{
				if (throwExceptions)
					throw;
			}
		}

		#region Implementation of ISnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected override Snapshot Get(Type aggregateRootType, string streamName)
		{
			MongoDbEventData query = MongoCollection
				.AsQueryable()
				.Where(snapshot => snapshot.AggregateId == streamName)
				.OrderByDescending(eventData => eventData.Version)
				.Take(1)
				.SingleOrDefault();

			return EventDeserialiser.Deserialise(query);
		}

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshot"/> to save and store.</param>
		public override void Save(Snapshot snapshot)
		{
			EventData eventData = BuildEventData(snapshot);
			var safeEventData = new MongoDbEventData(eventData);

			Logger.LogDebug("Adding an event to the MongoDB database", "MongoDbSnapshotStore\\Save");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.InsertOne(safeEventData);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the MongoDB database took {0}.", end - start), "MongoDbSnapshotStore\\Save");
			}
			finally
			{
				Logger.LogDebug("Adding an event to the MongoDB database... Done", "MongoDbSnapshotStore\\Save");
			}
		}

		#endregion
	}
}