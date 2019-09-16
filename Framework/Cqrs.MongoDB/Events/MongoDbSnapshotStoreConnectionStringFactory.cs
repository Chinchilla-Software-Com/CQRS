#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Configuration;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Exceptions;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A factory for getting connection strings and database names for Snapshot Store access.
	/// </summary>
	public class MongoDbSnapshotStoreConnectionStringFactory
		: IMongoDbSnapshotStoreConnectionStringFactory
	{
		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the name of the connection string of the MongoDB server.
		/// </summary>
		public static string MongoDbConnectionNameApplicationKey = "Cqrs.MongoDb.SnapshotStore.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the name of the database.
		/// </summary>
		public static string MongoDbDatabaseNameApplicationKey = "Cqrs.MongoDb.SnapshotStore.DatabaseName";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="MongoDbSnapshotStoreConnectionStringFactory"/>.
		/// </summary>
		public MongoDbSnapshotStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		#region Implementation of IMongoDbSnapshotStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		public string GetSnapshotStoreConnectionString()
		{
			Logger.LogDebug("Getting MongoDB connection string", "MongoDbSnapshotStoreConnectionStringFactory\\GetSnapshotStoreConnectionString");

			try
			{
				string applicationKey;

				if (!ConfigurationManager.TryGetSetting(MongoDbConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", MongoDbConnectionNameApplicationKey), "MongoDbSnapshotStoreConnectionStringFactory\\GetSnapshotStoreConnectionString");
					throw new MissingApplicationSettingForConnectionStringException(MongoDbConnectionNameApplicationKey);
				}

				ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey];
				// If the connection string doesn't exist this value IS the connection string itself
				if (connectionString == null)
					throw new MissingConnectionStringException(applicationKey);

				return connectionString.ConnectionString;
			}
			finally
			{
				Logger.LogDebug("Getting MongoDB connection string... Done", "MongoDbSnapshotStoreConnectionStringFactory\\GetSnapshotStoreConnectionString");
			}
		}

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public string GetSnapshotStoreDatabaseName()
		{
			string applicationKey;

			if (!ConfigurationManager.TryGetSetting(MongoDbDatabaseNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				throw new MissingApplicationSettingException(MongoDbDatabaseNameApplicationKey);
			return applicationKey;
		}

		#endregion
	}
}