using System;
using Cqrs.Configuration;
using Cqrs.Logging;

namespace Cqrs.Mongo.Factories
{
	public class MongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "CqrsMongoDb";

		public static string MongoDbtabaseNameKey = "CqrsMongoDbDatabaseName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILog Logger { get; private set; }

		public MongoDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILog logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public string GetMongoConnectionString()
		{
			Logger.LogInfo("Getting MongoDB connection string", "MongoDataStoreConnectionStringFactory\\GetMongoConnectionString");
			try
			{
				return ConfigurationManager.GetSetting(MongoDbConnectionStringKey) ?? System.Configuration.ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No connection string named '{0}' in the configuration file.", MongoDbConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogInfo("Getting MongoDB connection string... Done", "MongoDataStoreConnectionStringFactory\\GetMongoConnectionString");
			}
		}

		public string GetMongoDatabaseName()
		{
			return ConfigurationManager.GetSetting(MongoDbtabaseNameKey);
		}
	}
}