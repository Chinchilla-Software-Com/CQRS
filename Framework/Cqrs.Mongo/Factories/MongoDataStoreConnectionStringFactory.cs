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
using Cqrs.Exceptions;

namespace Cqrs.Mongo.Factories
{
	/// <summary>
	/// A factory for MongoDb related connection string settings.
	/// </summary>
	public class MongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
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
		/// Instantiate a new instance of <see cref="MongoDataStoreConnectionStringFactory"/>.
		/// </summary>
		public MongoDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		/// <summary>
		/// Get the connection string for the MongoDB server.
		/// </summary>
		public string GetMongoConnectionString()
		{
			Logger.LogInfo("Getting MongoDB connection string", "MongoDataStoreConnectionStringFactory\\GetMongoConnectionString");
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
				Logger.LogDebug("Getting MongoDB connection string... Done", "MongoDataStoreConnectionStringFactory\\GetMongoConnectionString");
			}
		}

		/// <summary>
		/// Get the name of database on the MongoDB server.
		/// </summary>
		public string GetMongoDatabaseName()
		{
			string databaseName = ConfigurationManager.GetSetting(MongoDbDatabaseNameKey) ?? ConfigurationManager.GetSetting(OldMongoDbDatabaseNameKey);
			if (string.IsNullOrEmpty(databaseName))
				throw new MissingApplicationSettingException(MongoDbDatabaseNameKey);
			return databaseName;
		}
	}
}