using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public class AzureDocumentDbDataStoreConnectionStringFactory : IAzureDocumentDbDataStoreConnectionStringFactory
	{
		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		public AzureDocumentDbDataStoreConnectionStringFactory(ILogger logger, IConfigurationManager configurationManager)
		{
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

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

		public virtual string GetAzureDocumentDbDatabaseName()
		{
			return ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.DatabaseName") ?? "CqrsStore";
		}

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

		protected virtual Uri GetAzureDocumentDbConnectionUrl()
		{
			return new Uri(ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.Url"));
		}

		protected virtual string GetAzureDocumentDbAuthorisationKey()
		{
			return ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.AuthorisationKey");
		}
	}
}