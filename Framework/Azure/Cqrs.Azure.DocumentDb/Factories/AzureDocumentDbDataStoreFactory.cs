#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using cdmdotnet.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.Factories
{
	/// <summary>
	/// A factory for obtaining DataStore collections from Azure DocumentDB
	/// </summary>
	public class AzureDocumentDbDataStoreFactory
	{
		protected IAzureDocumentDbDataStoreConnectionStringFactory AzureDocumentDbDataStoreConnectionStringFactory { get; private set; }

		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		public AzureDocumentDbDataStoreFactory(IAzureDocumentDbDataStoreConnectionStringFactory azureDocumentDbDataStoreConnectionStringFactory, IAzureDocumentDbHelper azureDocumentDbHelper, ILogger logger)
		{
			AzureDocumentDbDataStoreConnectionStringFactory = azureDocumentDbDataStoreConnectionStringFactory;
			AzureDocumentDbHelper = azureDocumentDbHelper;
			Logger = logger;
		}

		protected virtual DocumentClient GetClient()
		{
			DocumentClient client = AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbConnectionClient();

			return client;
		}

		protected virtual DocumentCollection GetCollection<TEntity>(DocumentClient client, Database database)
		{
			string collectionName = string.Format(AzureDocumentDbDataStoreConnectionStringFactory.UseSingleCollectionForAllDataStores() ? "{0}" : "{0}_{1}", AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbCollectionName(), typeof(TEntity).FullName);
			DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName).Result;

			return collection;
		}

		protected virtual IOrderedQueryable<TEntity> GetQuery<TEntity>(DocumentClient client, DocumentCollection collection)
		{
			Logger.LogDebug("Getting Azure query", "AzureDocumentDbDataStoreFactory\\GetQuery");
			try
			{
				IOrderedQueryable<TEntity> query = client.CreateDocumentQuery<TEntity>(collection.SelfLink);

				return query;
			}
			finally
			{
				Logger.LogDebug("Getting Azure query... Done", "AzureDocumentDbDataStoreFactory\\GetQuery");
			}
		}

		protected virtual Database GetDatabase(DocumentClient client)
		{
			return AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbDatabaseName()).Result;
		}
	}
}