using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public class AzureDocumentDbDataStoreConnectionStringFactory : IAzureDocumentDbDataStoreConnectionStringFactory
	{
		public virtual DocumentClient GetAzureDocumentDbConnectionString()
		{
			return new DocumentClient(GetAzureDocumentDbConnectionUrl(), GetAzureDocumentDbAuthorisationKey());
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