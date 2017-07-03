#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using cdmdotnet.Logging;
using Cqrs.Configuration;

namespace Cqrs.MongoDB.Events
{
	public class MongoDbEventStoreConnectionStringFactory : IMongoDbEventStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "CqrsMongoDbEventStore";

		public static string MongoDbConnectionNameApplicationKey = "Cqrs.MongoDb.EventStore.ConnectionStringName";

		public static string MongoDbDatabaseNameKey = "CqrsMongoDbEventStoreDatabaseName";

		public static string MongoDbDatabaseNameApplicationKey = "Cqrs.MongoDb.EventStore.DatabaseName";

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
			Logger.LogDebug("Getting MongoDB connection string", "MongoDbEventStoreConnectionStringFactory\\GetEventStoreConnectionString");

			try
			{
				string applicationKey;

				if (!ConfigurationManager.TryGetSetting(MongoDbConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", MongoDbConnectionNameApplicationKey), "MongoDbEventStoreConnectionStringFactory\\GetEventStoreConnectionString");

					if (!ConfigurationManager.TryGetSetting(MongoDbConnectionStringKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
					{
						Logger.LogDebug(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", MongoDbConnectionStringKey), "MongoDbEventStoreConnectionStringFactory\\GetEventStoreConnectionString");
						throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", MongoDbConnectionNameApplicationKey));
					}
				}

				ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey];
				// If the connection string doesn't exist this value IS the connection string itself
				if (connectionString == null)
					return applicationKey;

				return connectionString.ConnectionString;
			}
			finally
			{
				Logger.LogDebug("Getting MongoDB connection string... Done", "MongoDbEventStoreConnectionStringFactory\\GetEventStoreConnectionString");
			}
		}

		public string GetEventStoreDatabaseName()
		{
			string applicationKey;

			if (!ConfigurationManager.TryGetSetting(MongoDbDatabaseNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
			{
				if (!ConfigurationManager.TryGetSetting(MongoDbDatabaseNameKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				{
					throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the name of the collection.", MongoDbDatabaseNameApplicationKey));
				}
			}
			return applicationKey;
		}

		#endregion
	}
}