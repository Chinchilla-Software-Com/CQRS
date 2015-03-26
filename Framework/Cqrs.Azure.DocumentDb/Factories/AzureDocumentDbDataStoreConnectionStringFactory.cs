using System;
using Cqrs.Logging;
using Microsoft.Azure;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public class AzureDocumentDbDataStoreConnectionStringFactory : IAzureDocumentDbDataStoreConnectionStringFactory
	{
		protected ILog Logger { get; private set; }

		public AzureDocumentDbDataStoreConnectionStringFactory(ILog logger)
		{
			Logger = logger;
		}

		public virtual DocumentClient GetAzureDocumentDbConnectionClient()
		{
			Logger.LogInfo("Getting Azure document client", "AzureDocumentDbDataStoreConnectionStringFactory\\GetAzureDocumentDbConnectionClient");
			try
			{
				return new DocumentClient(GetAzureDocumentDbConnectionUrl(), GetAzureDocumentDbAuthorisationKey());
			}
			finally
			{
				Logger.LogInfo("Getting Azure document client... Done", "AzureDocumentDbDataStoreConnectionStringFactory\\GetAzureDocumentDbConnectionClient");
			}
		}

		public virtual string GetAzureDocumentDbDatabaseName()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.Azure.DocumentDb.DabaseName") ?? "CqrsStore";
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