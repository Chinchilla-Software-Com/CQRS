using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cqrs.Mongo.DataStores;
using Cqrs.Mongo.DataStores.Indexes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Cqrs.Mongo.Factories
{
	/// <summary>
	/// A factory for obtaining DataStore collections from Mongo
	/// </summary>
	public class MongoDataStoreFactory
	{
		private static IDictionary<Type, IList<object>> IndexTypesByEntityType { get; set; }

		protected IMongoDataStoreConnectionStringFactory MongoDataStoreConnectionStringFactory { get; private set; }

		public MongoDataStoreFactory(IMongoDataStoreConnectionStringFactory mongoDataStoreConnectionStringFactory)
		{
			MongoDataStoreConnectionStringFactory = mongoDataStoreConnectionStringFactory;
		}

		static MongoDataStoreFactory()
		{
			IndexTypesByEntityType = new Dictionary<Type, IList<object>>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type mongoIndexType in assembly.GetTypes().Where(type => typeof(MongoIndex<>).IsAssignableFrom(type)))
				{
					IList<object> indexTypes;
					if (!IndexTypesByEntityType.TryGetValue(mongoIndexType, out indexTypes))
						IndexTypesByEntityType.Add(mongoIndexType, indexTypes = new List<object>());
					object mongoIndex = Activator.CreateInstance(mongoIndexType, true);
					indexTypes.Add(mongoIndex);
				}
			}
		}

		protected virtual MongoCollection<TEntity> GetCollection<TEntity>()
		{
			var mongoClient = new MongoClient(MongoDataStoreConnectionStringFactory.GetMongoConnectionString());
			MongoServer mongoServer = mongoClient.GetServer();
			MongoDatabase mongoDatabase = mongoServer.GetDatabase(MongoDataStoreConnectionStringFactory.GetMongoDatabaseName());

			return mongoDatabase.GetCollection<TEntity>(typeof(TEntity).FullName);
		}

		protected virtual void VerifyIndexes<TEntity>(MongoCollection<TEntity> collection)
		{
			foreach (IList<object> untypedIndexTypes in IndexTypesByEntityType[typeof(MongoIndex<TEntity>)])
			{
				foreach (object untypedIndexType in untypedIndexTypes)
				{
					var mongoIndex = (MongoIndex<TEntity>) untypedIndexType;

					var indexKeysBuilder = new IndexKeysBuilder();
					if (mongoIndex.IsAcending)
						indexKeysBuilder = indexKeysBuilder.Ascending(mongoIndex.Selectors.ToArray());
					else
						indexKeysBuilder = indexKeysBuilder.Descending(mongoIndex.Selectors.ToArray());

					collection.EnsureIndex
					(
						indexKeysBuilder,
						IndexOptions
							.SetUnique(mongoIndex.IsUnique)
							.SetName(mongoIndex.Name)
					);
				}
			}
		}
	}
}