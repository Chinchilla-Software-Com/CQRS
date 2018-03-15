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
using Cqrs.Events;
using Cqrs.Exceptions;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A factory for getting connection strings and database names for <see cref="IEventStore{TAuthenticationToken}"/> access.
	/// </summary>
	public class MongoDbEventStoreConnectionStringFactory : IMongoDbEventStoreConnectionStringFactory
	{
		/// <summary>
		/// Backwards compatibility with version 1.
		/// </summary>
		public static string MongoDbConnectionStringKey = "CqrsMongoDbEventStore";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the name of the connection string of the MongoDB server.
		/// </summary>
		public static string MongoDbConnectionNameApplicationKey = "Cqrs.MongoDb.EventStore.ConnectionStringName";

		/// <summary>
		/// Backwards compatibility with version 1.
		/// </summary>
		public static string MongoDbDatabaseNameKey = "CqrsMongoDbEventStoreDatabaseName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the name of the database.
		/// </summary>
		public static string MongoDbDatabaseNameApplicationKey = "Cqrs.MongoDb.EventStore.DatabaseName";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="MongoDbEventStoreConnectionStringFactory"/>.
		/// </summary>
		public MongoDbEventStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		#region Implementation of IMongoDbEventStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string.
		/// </summary>
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
						throw new MissingApplicationSettingForConnectionStringException(MongoDbConnectionNameApplicationKey);
					}
				}

				ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey];
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

		/// <summary>
		/// Gets the current database name.
		/// </summary>
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