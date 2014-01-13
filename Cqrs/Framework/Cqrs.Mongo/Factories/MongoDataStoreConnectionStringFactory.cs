using System.Configuration;

namespace Cqrs.Mongo.Factories
{
	public class MongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
	{
		private const string MongoDbConnectionStringKey = "CqrsMongoDb";

		private const string DatabaseName = "Cqrs";

		public string GetMongoConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
		}

		public string GetMongoDatabaseName()
		{
			return DatabaseName;
		}
	}
}