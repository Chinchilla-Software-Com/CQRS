#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;

namespace Cqrs.MongoDB.Events
{
	public class MongoDbEventStoreConnectionStringFactory : IMongoDbEventStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "CqrsMongoDbEventStore";

		public static string MongoDbDatabaseNameKey = "CqrsMongoDbEventStoreDatabaseName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public MongoDbEventStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		#region Implementation of IMongoDbEventStoreConnectionStringFactory

		public string GetEventStoreConnectionString()
		{
			Logger.LogInfo("Getting MongoDB connection string", "MongoDbEventStoreConnectionStringFactory\\GetEventStoreConnectionString");
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
				Logger.LogInfo("Getting MongoDB connection string... Done", "MongoDbEventStoreConnectionStringFactory\\GetEventStoreConnectionString");
			}
		}

		public string GetEventStoreDatabaseName()
		{
			return ConfigurationManager.GetSetting(MongoDbDatabaseNameKey);
		}

		#endregion
	}
}