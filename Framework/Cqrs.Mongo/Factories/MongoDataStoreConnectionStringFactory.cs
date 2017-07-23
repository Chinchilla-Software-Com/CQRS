#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Exceptions;

namespace Cqrs.Mongo.Factories
{
	public class MongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "Cqrs.MongoDb.DataStore.ConnectionStringName";

		public static string OldMongoDbConnectionStringKey = "CqrsMongoDb";

		public static string OldMongoDbDatabaseNameKey = "CqrsMongoDbDatabaseName";

		public static string MongoDbDatabaseNameKey = "Cqrs.MongoDb.DataStore.DatabaseName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public MongoDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public string GetMongoConnectionString()
		{
			Logger.LogInfo("Getting MongoDB connection string", "MongoDataStoreConnectionStringFactory\\GetMongoConnectionString");
			try
			{
				string appKey;
				if (ConfigurationManager.TryGetSetting(MongoDbConnectionStringKey, out appKey))
				{
					try
					{
						return System.Configuration.ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
					}
					catch (Exception exception)
					{
						throw new MissingApplicationSettingForConnectionStringException(MongoDbConnectionStringKey, exception);
					}
				}
				if (ConfigurationManager.TryGetSetting(OldMongoDbConnectionStringKey, out appKey))
					return appKey;
				throw new MissingApplicationSettingForConnectionStringException(MongoDbConnectionStringKey);
			}
			finally
			{
				Logger.LogDebug("Getting MongoDB connection string... Done", "MongoDataStoreConnectionStringFactory\\GetMongoConnectionString");
			}
		}

		public string GetMongoDatabaseName()
		{
			string databaseName = ConfigurationManager.GetSetting(MongoDbDatabaseNameKey) ?? ConfigurationManager.GetSetting(OldMongoDbDatabaseNameKey);
			if (string.IsNullOrEmpty(databaseName))
				throw new MissingApplicationSettingException(MongoDbDatabaseNameKey);
			return databaseName;
		}
	}
}