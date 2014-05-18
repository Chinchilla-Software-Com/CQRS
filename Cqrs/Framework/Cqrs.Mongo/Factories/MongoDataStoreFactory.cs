using System;
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
		protected IMongoDataStoreConnectionStringFactory MongoDataStoreConnectionStringFactory { get; private set; }

		public MongoDataStoreFactory(IMongoDataStoreConnectionStringFactory mongoDataStoreConnectionStringFactory)
		{
			MongoDataStoreConnectionStringFactory = mongoDataStoreConnectionStringFactory;
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
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type mongoIndexType in assembly.GetTypes().Where(type => typeof(MongoIndex<TEntity>).IsAssignableFrom(type)))
				{
					var mongoIndex = (MongoIndex<TEntity>) Activator.CreateInstance(mongoIndexType, true);

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