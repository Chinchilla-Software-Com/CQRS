﻿#region Copyright
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
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Messages;
using Cqrs.MongoDB.DataStores.Indexes;
using Cqrs.MongoDB.Events.Indexes;
using Cqrs.MongoDB.Factories;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A MongoDB based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class MongoDbEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the <see cref="IMongoCollection{TData}"/>
		/// </summary>
		protected IMongoCollection<MongoDbEventData> MongoCollection { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IMongoDbEventStoreConnectionStringFactory"/>
		/// </summary>
		protected IMongoDbEventStoreConnectionStringFactory MongoDbEventStoreConnectionStringFactory { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

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

		/// <summary>
		/// Instantiate a new instance of <see cref="MongoDbEventStore{TAuthenticationToken}"/>
		/// triggering any require index checks.
		/// </summary>
		public MongoDbEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IMongoDbEventStoreConnectionStringFactory mongoDbEventStoreConnectionStringFactory, IConfigurationManager configurationManager)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			MongoDbEventStoreConnectionStringFactory = mongoDbEventStoreConnectionStringFactory;
			ConfigurationManager = configurationManager;

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
			var mongoClient = new MongoClient(MongoDbEventStoreConnectionStringFactory.GetEventStoreConnectionString());
			IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoDbEventStoreConnectionStringFactory.GetEventStoreDatabaseName());

			return mongoDatabase.GetCollection<MongoDbEventData>(MongoDbEventStoreConnectionStringFactory.GetEventStoreDatabaseName());
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
			if (!bool.TryParse(ConfigurationManager.GetSetting("Cqrs.MongoDb.EventStore.ThrowExceptionsOnIndexPreparation"), out throwExceptions))
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

		#region Overrides of EventStore<TAuthenticationToken>

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		public override
#if NET45
			IEnumerable<IEvent<TAuthenticationToken>> Get
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync
#endif
				(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<MongoDbEventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.AggregateId == streamName && eventData.Version > fromVersion)
				.OrderByDescending(eventData => eventData.Version);

			if (useLastEventOnly)
				query = query.AsQueryable().Take(1);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();

			return
#if NET45
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public override
#if NET45
			IEnumerable<IEvent<TAuthenticationToken>> GetToVersion
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToVersionAsync
#endif
				(Type aggregateRootType, Guid aggregateId, int version)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<MongoDbEventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.AggregateId == streamName && eventData.Version <= version)
				.OrderByDescending(eventData => eventData.Version);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();

			return
#if NET45
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="System.DateTime"/></param>
		public override
#if NET45
			IEnumerable<IEvent<TAuthenticationToken>> GetToDate
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToDateAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<MongoDbEventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp <= versionedDate)
				.OrderByDescending(eventData => eventData.Version);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();

			return
#if NET45
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="System.Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="System.DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="System.DateTime"/></param>
		public override
#if NET45
			IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetBetweenDatesAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			IEnumerable<MongoDbEventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp >= fromVersionedDate && eventData.Timestamp <= toVersionedDate)
				.OrderByDescending(eventData => eventData.Version);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();

			return
#if NET45
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public override
#if NET45
			IEnumerable<EventData> Get
#else
			async Task<IEnumerable<EventData>> GetAsync
#endif
				(Guid correlationId)
		{
			IEnumerable<MongoDbEventData> query = MongoCollection
				.AsQueryable()
				.Where(eventData => eventData.CorrelationId == correlationId)
				.OrderBy(eventData => eventData.Timestamp);

			var results = query.ToList();

			return
#if NET45
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override
#if NET45
			void PersistEvent
#else
			async Task PersistEventAsync
#endif
				(EventData eventData)
		{
			var safeEventData = eventData as MongoDbEventData;
			if (safeEventData == null)
				safeEventData = new MongoDbEventData(eventData);
			Logger.LogDebug("Adding an event to the MongoDB database", "MongoDbEventStore\\PersistEvent");
			try
			{
				DateTime start = DateTime.Now;
#if NET45
				MongoCollection.InsertOne
#else
				await MongoCollection.InsertOneAsync
#endif
					(safeEventData);
				DateTime end = DateTime.Now;
				Logger.LogDebug($"Adding data in the MongoDB database took {end - start}.", "MongoDbEventStore\\PersistEvent");
			}
			finally
			{
				Logger.LogDebug("Adding an event to the MongoDB database... Done", "MongoDbEventStore\\PersistEvent");
			}
		}

#endregion
	}
}