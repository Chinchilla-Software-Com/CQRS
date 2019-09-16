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
using Cqrs.Events;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	/// <summary>
	/// A factory for getting connections and database names for <see cref="IEventStore{TAuthenticationToken}"/> access.
	/// </summary>
	public class AzureDocumentDbEventStoreConnectionStringFactory
		: IAzureDocumentDbEventStoreConnectionStringFactory
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
		/// Instantiates a new instance of <see cref="AzureDocumentDbEventStoreConnectionStringFactory"/>.
		/// </summary>
		public AzureDocumentDbEventStoreConnectionStringFactory(ILogger logger, IConfigurationManager configurationManager)
		{
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// Gets the current <see cref="DocumentClient"/>.
		/// </summary>
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

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public virtual string GetEventStoreConnectionDatabaseName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.DatabaseName") ?? ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.LogStreamName") ?? "CqrsEventStore";
		}

		/// <summary>
		/// Gets the current collection name.
		/// </summary>
		public string GetEventStoreConnectionCollectionName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.CollectionName") ?? "CqrsEventStore";
		}

		/// <summary>
		/// Gets the current connection <see cref="Uri"/> from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual Uri GetEventStoreConnectionUrl()
		{
			return new Uri(ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.Url"));
		}

		/// <summary>
		/// Gets the current connection authorisation key from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual string GetEventStoreConnectionAuthorisationKey()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStore.Azure.DocumentDb.AuthorisationKey");
		}
	}
}