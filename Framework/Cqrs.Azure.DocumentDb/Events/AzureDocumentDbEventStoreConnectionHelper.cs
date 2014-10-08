using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure;

namespace Cqrs.Azure.DocumentDb.Events
{
	public class AzureDocumentDbEventStoreConnectionHelper : IAzureDocumentDbEventStoreConnectionHelper
	{
		public virtual DocumentClient GetEventStoreConnection()
		{
			return new DocumentClient(GetEventStoreConnectionUrl(), GetEventStoreConnectionAuthorisationKey());
		}

		public virtual string GetEventStoreConnectionLogStreamName()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.LogStreamName") ?? "CqrsEventStore";
		}

		protected virtual Uri GetEventStoreConnectionUrl()
		{
			return new Uri(CloudConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.Url"));
		}

		protected virtual string GetEventStoreConnectionAuthorisationKey()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.AuthorisationKey");
		}
	}
}