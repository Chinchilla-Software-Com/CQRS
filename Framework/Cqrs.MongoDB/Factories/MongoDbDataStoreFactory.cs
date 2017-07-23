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
using cdmdotnet.Logging;
using Cqrs.DataStores;
using Cqrs.MongoDB.DataStores.Indexes;
using Cqrs.MongoDB.Serialisers;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace Cqrs.MongoDB.Factories
{
	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> collections from Mongo
	/// </summary>
	public class MongoDbDataStoreFactory
	{
		internal static IDictionary<Type, IList<object>> IndexTypesByEntityType { get; set; }

		protected ILogger Logger { get; private set; }

		protected IMongoDbDataStoreConnectionStringFactory MongoDbDataStoreConnectionStringFactory { get; private set; }

		public MongoDbDataStoreFactory(ILogger logger, IMongoDbDataStoreConnectionStringFactory mongoDbDataStoreConnectionStringFactory)
		{
			Logger = logger;
			MongoDbDataStoreConnectionStringFactory = mongoDbDataStoreConnectionStringFactory;
		}

		static MongoDbDataStoreFactory()
		{
			var typeSerializer = new TypeSerialiser();
			BsonSerializer.RegisterSerializer(typeof(Type), typeSerializer);
			BsonSerializer.RegisterSerializer(Type.GetType("System.RuntimeType"), typeSerializer);

			IndexTypesByEntityType = new Dictionary<Type, IList<object>>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var mongoIndexTypes = assembly
					.GetTypes()
					.Where
					(
						type =>
							(
								// base is NOT an abstract index
								(
									type.BaseType != null &&
									type.BaseType.IsGenericType &&
									typeof(MongoDbIndex<>).IsAssignableFrom(type.BaseType.GetGenericTypeDefinition())
								)
								||
								// base is an abstract index
								(
									type.BaseType != null &&
									type.BaseType.BaseType != null &&
									type.BaseType.BaseType.IsGenericType &&
									typeof(MongoDbIndex<>).IsAssignableFrom(type.BaseType.BaseType.GetGenericTypeDefinition())
								)
								||
								// a deeper inheritance model
								(
									type.BaseType != null &&
									type.BaseType.BaseType != null &&
									type.BaseType.BaseType.BaseType != null &&
									type.BaseType.BaseType.BaseType.IsGenericType &&
									typeof(MongoDbIndex<>).IsAssignableFrom(type.BaseType.BaseType.BaseType.GetGenericTypeDefinition())
								)
								||
								// an even deeper inheritance model
								(
									type.BaseType != null &&
									type.BaseType.BaseType != null &&
									type.BaseType.BaseType.BaseType != null &&
									type.BaseType.BaseType.BaseType.BaseType != null &&
									type.BaseType.BaseType.BaseType.BaseType.IsGenericType &&
									typeof(MongoDbIndex<>).IsAssignableFrom(type.BaseType.BaseType.BaseType.BaseType.GetGenericTypeDefinition())
								)
							)
							&& !type.IsAbstract
					);
				foreach (Type mongoIndexType in mongoIndexTypes)
				{
					Type genericType = mongoIndexType;
					while (genericType != null && !genericType.IsGenericType)
					{
						genericType = genericType.BaseType;
					}
					genericType = genericType.GetGenericArguments().Single();

					IList<object> indexTypes;
					if (!IndexTypesByEntityType.TryGetValue(genericType, out indexTypes))
						IndexTypesByEntityType.Add(genericType, indexTypes = new List<object>());
					object mongoIndex = Activator.CreateInstance(mongoIndexType, true);
					indexTypes.Add(mongoIndex);
				}
			}
		}

		protected virtual IMongoCollection<TEntity> GetCollection<TEntity>()
		{
			var mongoClient = new MongoClient(MongoDbDataStoreConnectionStringFactory.GetDataStoreConnectionString());
			IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoDbDataStoreConnectionStringFactory.GetDataStoreDatabaseName());

			return mongoDatabase.GetCollection<TEntity>(typeof(TEntity).FullName);
		}

		protected virtual void VerifyIndexes<TEntity>(IMongoCollection<TEntity> collection)
		{
			Type entityType = typeof (TEntity);
			if (IndexTypesByEntityType.ContainsKey(entityType))
			{
				foreach (object untypedIndexType in IndexTypesByEntityType[entityType])
				{
					var mongoIndex = (MongoDbIndex<TEntity>)untypedIndexType;

					IndexKeysDefinitionBuilder<TEntity> indexKeysBuilder = Builders<TEntity>.IndexKeys;
					IndexKeysDefinition<TEntity> indexKey = null;

					IList<Expression<Func<TEntity, object>>> selectors = mongoIndex.Selectors.ToList();
					for (int i = 0; i < selectors.Count; i++)
					{
						Expression<Func<TEntity, object>> expression = selectors[i];
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

					collection.Indexes.CreateOne
					(
						indexKey,
						new CreateIndexOptions
						{
							Unique = mongoIndex.IsUnique,
							Name = mongoIndex.Name
						}
					);
				}
			}
		}
	}
}