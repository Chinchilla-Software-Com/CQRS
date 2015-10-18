using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;
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

		public string GetAzureDocumentDbCollectionName()
		{
			return ConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.CollectionName") ?? "CqrsDataStore";
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