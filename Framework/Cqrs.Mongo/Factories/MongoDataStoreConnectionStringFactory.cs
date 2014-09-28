using System;
using System.Configuration;

namespace Cqrs.Mongo.Factories
{
	public class MongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "CqrsMongoDb";

		public static string MongoDbtabaseNameKey = "CqrsMongoDbDatabaseName";

		public string GetMongoConnectionString()
		{
			try
			{
				return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException("No connection string named 'CqrsMongoDb' in the configuration file.", exception);
			}
		}

		public string GetMongoDatabaseName()
		{
			return ConfigurationManager.AppSettings[MongoDbtabaseNameKey];
		}
	}
}