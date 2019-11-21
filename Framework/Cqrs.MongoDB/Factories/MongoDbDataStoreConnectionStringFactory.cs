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
using Chinchilla.Logging;
using Cqrs.DataStores;
using Cqrs.Exceptions;

namespace Cqrs.MongoDB.Factories
{
	/// <summary>
	/// A factory for getting connection strings and database names for <see cref="IDataStore{TData}"/> access.
	/// </summary>
	public class MongoDbDataStoreConnectionStringFactory : IMongoDbDataStoreConnectionStringFactory
	{
		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the name of the connection string of the MongoDB server.
		/// </summary>
		public static string MongoDbConnectionStringKey = "Cqrs.MongoDb.DataStore.ConnectionStringName";

		/// <summary>
		/// Backwards compatibility with version 1.
		/// </summary>
		public static string OldMongoDbConnectionStringKey = "CqrsMongoDb";

		/// <summary>
		/// Backwards compatibility with version 1.
		/// </summary>
		public static string OldMongoDbDatabaseNameKey = "CqrsMongoDbDatabaseName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the name of the database.
		/// </summary>
		public static string MongoDbDatabaseNameKey = "Cqrs.MongoDb.DataStore.DatabaseName";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="MongoDbDataStoreConnectionStringFactory"/>.
		/// </summary>
		public MongoDbDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		/// <summary>
		/// Gets the current connection string.
		/// </summary>
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

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public string GetDataStoreDatabaseName()
		{
			string databaseName = ConfigurationManager.GetSetting(MongoDbDatabaseNameKey) ?? ConfigurationManager.GetSetting(OldMongoDbDatabaseNameKey);
			if (string.IsNullOrEmpty(databaseName))
				throw new MissingApplicationSettingException(MongoDbDatabaseNameKey);
			return databaseName;
		}
	}
}