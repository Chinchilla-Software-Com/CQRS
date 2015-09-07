using System;
using cdmdotnet.Logging;
using Microsoft.Azure;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public class AzureDocumentDbDataStoreConnectionStringFactory : IAzureDocumentDbDataStoreConnectionStringFactory
	{
		protected ILogger Logger { get; private set; }

		public AzureDocumentDbDataStoreConnectionStringFactory(ILogger logger)
		{
			Logger = logger;
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
			return CloudConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.DatabaseName") ?? "CqrsStore";
		}

		public string GetAzureDocumentDbCollectionName()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.CollectionName") ?? "CqrsDataStore";
		}

		protected virtual Uri GetAzureDocumentDbConnectionUrl()
		{
			return new Uri(CloudConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.Url"));
		}

		protected virtual string GetAzureDocumentDbAuthorisationKey()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.AuthorisationKey");
		}
	}
}