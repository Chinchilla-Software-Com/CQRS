#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Configuration;
using cdmdotnet.Logging;

namespace Cqrs.MongoDB.Factories
{
	public class MongoDbDataStoreConnectionStringFactory : IMongoDbDataStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "CqrsMongoDb";

		public static string MongoDbDatabaseNameKey = "CqrsMongoDbDatabaseName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public MongoDbDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public string GetDataStoreConnectionString()
		{
			Logger.LogInfo("Getting MongoDB connection string", "MongoDataStoreConnectionStringFactory\\GetDataStoreConnectionString");
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
				Logger.LogInfo("Getting MongoDB connection string... Done", "MongoDataStoreConnectionStringFactory\\GetDataStoreConnectionString");
			}
		}

		public string GetDataStoreDatabaseName()
		{
			return ConfigurationManager.GetSetting(MongoDbDatabaseNameKey);
		}
	}
}