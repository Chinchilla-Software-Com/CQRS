#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.DataStores;
using Cqrs.Exceptions;

namespace Cqrs.MongoDB.Factories
{
	/// <summary>
	/// A factory for getting connection strings and database names for <see cref="IDataStore{TData}"/> access.
	/// </summary>
	public class MongoDbDataStoreConnectionStringFactory : IMongoDbDataStoreConnectionStringFactory
	{
		public static string MongoDbConnectionStringKey = "Cqrs.MongoDb.DataStore.ConnectionStringName";

		public static string OldMongoDbConnectionStringKey = "CqrsMongoDb";

		public static string OldMongoDbDatabaseNameKey = "CqrsMongoDbDatabaseName";

		public static string MongoDbDatabaseNameKey = "Cqrs.MongoDb.DataStore.DatabaseName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public MongoDbDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public string GetDataStoreConnectionString()
		{
			Logger.LogDebug("Getting MongoDB connection string", "MongoDbDataStoreConnectionStringFactory\\GetDataStoreConnectionString");
			try
			{
				string applicationKey;
				if (ConfigurationManager.TryGetSetting(MongoDbConnectionStringKey, out applicationKey) && !string.IsNullOrEmpty(applicationKey))
				{
					try
					{
						ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey];
						if (connectionString != null)
							return connectionString.ConnectionString;
					}
					catch (Exception exception)
					{
						throw new MissingConnectionStringException(applicationKey, exception);
					}
				}
				if (ConfigurationManager.TryGetSetting(OldMongoDbConnectionStringKey, out applicationKey) && !string.IsNullOrEmpty(applicationKey))
					return applicationKey;
				throw new MissingApplicationSettingForConnectionStringException(MongoDbConnectionStringKey);
			}
			finally
			{
				Logger.LogDebug("Getting MongoDB connection string... Done", "MongoDbDataStoreConnectionStringFactory\\GetDataStoreConnectionString");
			}
		}

		public string GetDataStoreDatabaseName()
		{
			string databaseName = ConfigurationManager.GetSetting(MongoDbDatabaseNameKey) ?? ConfigurationManager.GetSetting(OldMongoDbDatabaseNameKey);
			if (string.IsNullOrEmpty(databaseName))
				throw new MissingApplicationSettingException(MongoDbDatabaseNameKey);
			return databaseName;
		}
	}
}