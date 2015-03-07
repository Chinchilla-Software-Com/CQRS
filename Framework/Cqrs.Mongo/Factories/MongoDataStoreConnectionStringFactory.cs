using System;
using Cqrs.Configuration;

namespace Cqrs.Mongo.Factories
{
	public class MongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "CqrsMongoDb";

		public static string MongoDbtabaseNameKey = "CqrsMongoDbDatabaseName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		public MongoDataStoreConnectionStringFactory(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		public string GetMongoConnectionString()
		{
			try
			{
				return ConfigurationManager.GetSetting(MongoDbConnectionStringKey) ?? System.Configuration.ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException("No connection string named 'CqrsMongoDb' in the configuration file.", exception);
			}
		}

		public string GetMongoDatabaseName()
		{
			return ConfigurationManager.GetSetting(MongoDbtabaseNameKey);
		}
	}
}