#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Snapshots;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	/// <summary>
	/// A factory for getting connections and database names for <see cref="ISnapshotStore"/> access.
	/// </summary>
	public class AzureDocumentDbSnapshotStoreConnectionStringFactory
		: IAzureDocumentDbSnapshotStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureDocumentDbSnapshotStoreConnectionStringFactory"/>.
		/// </summary>
		public AzureDocumentDbSnapshotStoreConnectionStringFactory(ILogger logger, IConfigurationManager configurationManager)
		{
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// Gets the current <see cref="DocumentClient"/>.
		/// </summary>
		public virtual DocumentClient GetSnapshotStoreConnectionClient()
		{
			Logger.LogDebug("Getting Azure document client", "AzureDocumentDbSnapshotStoreConnectionStringFactory\\GetSnapshotStoreConnectionClient");
			try
			{
				return new DocumentClient(GetSnapshotStoreConnectionUrl(), GetSnapshotStoreConnectionAuthorisationKey());
			}
			finally
			{
				Logger.LogDebug("Getting Azure document client... Done", "AzureDocumentDbSnapshotStoreConnectionStringFactory\\GetSnapshotStoreConnectionClient");
			}
		}

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public virtual string GetSnapshotStoreConnectionDatabaseName()
		{
			return ConfigurationManager.GetSetting("Cqrs.SnapshotStore.Azure.DocumentDb.DatabaseName") ?? "CqrsSnapshotStore";
		}

		/// <summary>
		/// Gets the current collection name.
		/// </summary>
		public string GetSnapshotStoreConnectionCollectionName()
		{
			return ConfigurationManager.GetSetting("Cqrs.SnapshotStore.Azure.DocumentDb.CollectionName") ?? "CqrsSnapshotStore";
		}

		/// <summary>
		/// Gets the current connection <see cref="Uri"/> from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual Uri GetSnapshotStoreConnectionUrl()
		{
			return new Uri(ConfigurationManager.GetSetting("Cqrs.SnapshotStore.Azure.DocumentDb.Url"));
		}

		/// <summary>
		/// Gets the current connection authorisation key from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual string GetSnapshotStoreConnectionAuthorisationKey()
		{
			return ConfigurationManager.GetSetting("Cqrs.SnapshotStore.Azure.DocumentDb.AuthorisationKey");
		}
	}
}