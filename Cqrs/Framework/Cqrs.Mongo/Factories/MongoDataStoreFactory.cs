using MongoDB.Driver;

namespace Cqrs.Mongo.Factories
{
	/// <summary>
	/// A factory for obtaining DataStore collections from Mongo
	/// </summary>
	internal class MongoDataStoreFactory
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
	}
}