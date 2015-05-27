using System;
using cdmdotnet.Logging;
using Microsoft.Azure;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	public class AzureDocumentDbEventStoreConnectionStringFactory : IAzureDocumentDbEventStoreConnectionStringFactory
	{
		protected ILogger Logger { get; private set; }

		public AzureDocumentDbEventStoreConnectionStringFactory(ILogger logger)
		{
			Logger = logger;
		}

		public virtual DocumentClient GetEventStoreConnectionClient()
		{
			Logger.LogInfo("Getting Azure document client", "AzureDocumentDbEventStoreConnectionStringFactory\\GetEventStoreConnectionClient");
			try
			{
				return new DocumentClient(GetEventStoreConnectionUrl(), GetEventStoreConnectionAuthorisationKey());
			}
			finally
			{
				Logger.LogInfo("Getting Azure document client... Done", "AzureDocumentDbEventStoreConnectionStringFactory\\GetEventStoreConnectionClient");
			}
		}

		public virtual string GetEventStoreConnectionDatabaseName()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.DatabaseName") ?? CloudConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.LogStreamName") ?? "CqrsEventStore";
		}

		public string GetEventStoreConnectionCollectionName()
		{
			return CloudConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.CollectionName") ?? "CqrsEventStore";
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