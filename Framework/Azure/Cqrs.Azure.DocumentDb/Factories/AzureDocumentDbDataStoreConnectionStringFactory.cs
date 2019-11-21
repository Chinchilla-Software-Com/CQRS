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
using Cqrs.DataStores;
using Cqrs.Entities;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	/// <summary>
	/// A factory for getting connections and database names for <see cref="IDataStore{TAuthenticationToken}"/> access.
	/// </summary>
	public class AzureDocumentDbDataStoreConnectionStringFactory : IAzureDocumentDbDataStoreConnectionStringFactory
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
		/// Instantiates a new instance of <see cref="AzureDocumentDbDataStoreConnectionStringFactory"/>.
		/// </summary>
		public AzureDocumentDbDataStoreConnectionStringFactory(ILogger logger, IConfigurationManager configurationManager)
		{
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// Gets the current <see cref="DocumentClient"/>.
		/// </summary>
		public virtual DocumentClient GetAzureDocumentDbConnectionClient()
		{
			Logger.LogDebug("Getting Azure document client", "AzureDocumentDbDataStoreConnectionStringFactory\\GetAzureDocumentDbConnectionClient");
			try
			{
				return new DocumentClient(GetAzureDocumentDbConnectionUrl(), GetAzureDocumentDbAuthorisationKey());
			}
			finally
			{
				Logger.LogDebug("Getting Azure document client... Done", "AzureDocumentDbDataStoreConnectionStringFactory\\GetAzureDocumentDbConnectionClient");
			}
		}

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public virtual string GetAzureDocumentDbDatabaseName()
		{
			return ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.DatabaseName") ?? "CqrsStore";
		}

		/// <summary>
		/// Gets the current collection name.
		/// </summary>
		public virtual string GetAzureDocumentDbCollectionName()
		{
			return ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.CollectionName") ?? "CqrsDataStore";
		}

		/// <summary>
		/// Indicates if a different collection should be used per <see cref="IEntity"/>/<see cref="IDataStore{TData}"/> or a single collection used for all instances of <see cref="IDataStore{TData}"/> and <see cref="IDataStore{TData}"/>.
		/// Setting this to true can become expensive as each <see cref="IEntity"/> will have it's own collection. Check the relevant SDK/pricing models.
		/// </summary>
		public virtual bool UseSingleCollectionForAllDataStores()
		{
			bool value;
			if (!bool.TryParse(ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.UseSingleCollectionForAllDataStores"), out value))
				value = true;
			return value;
		}

		/// <summary>
		/// Gets the current connection <see cref="Uri"/> from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual Uri GetAzureDocumentDbConnectionUrl()
		{
			return new Uri(ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.Url"));
		}

		/// <summary>
		/// Gets the current connection authorisation key from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual string GetAzureDocumentDbAuthorisationKey()
		{
			return ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.AuthorisationKey");
		}
	}
}