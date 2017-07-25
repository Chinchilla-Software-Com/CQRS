#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	public class AzureDocumentDbEventStoreConnectionStringFactory : IAzureDocumentDbEventStoreConnectionStringFactory
	{
		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		public AzureDocumentDbEventStoreConnectionStringFactory(ILogger logger, IConfigurationManager configurationManager)
		{
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

		public virtual DocumentClient GetEventStoreConnectionClient()
		{
			Logger.LogDebug("Getting Azure document client", "AzureDocumentDbEventStoreConnectionStringFactory\\GetEventStoreConnectionClient");
			try
			{
				return new DocumentClient(GetEventStoreConnectionUrl(), GetEventStoreConnectionAuthorisationKey());
			}
			finally
			{
				Logger.LogDebug("Getting Azure document client... Done", "AzureDocumentDbEventStoreConnectionStringFactory\\GetEventStoreConnectionClient");
			}
		}

		public virtual string GetEventStoreConnectionDatabaseName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.DatabaseName") ?? ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.LogStreamName") ?? "CqrsEventStore";
		}

		public string GetEventStoreConnectionCollectionName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.CollectionName") ?? "CqrsEventStore";
		}

		protected virtual Uri GetEventStoreConnectionUrl()
		{
			return new Uri(ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.Url"));
		}

		protected virtual string GetEventStoreConnectionAuthorisationKey()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.AuthorisationKey");
		}
	}
}