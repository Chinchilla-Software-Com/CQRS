#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using Cqrs.Logging;
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

		protected ILog Logger { get; private set; }

		public AzureDocumentDbDataStoreFactory(IAzureDocumentDbDataStoreConnectionStringFactory azureDocumentDbDataStoreConnectionStringFactory, IAzureDocumentDbHelper azureDocumentDbHelper, ILog logger)
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

		protected virtual DocumentCollection GetCollection(DocumentClient client, Database database)
		{
			DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, "CqrsDataStore").Result;

			return collection;
		}

		protected virtual IOrderedQueryable<TEntity> GetQuery<TEntity>(DocumentClient client, DocumentCollection collection)
		{
			Logger.LogInfo("Getting Azure query", "AzureDocumentDbDataStoreFactory\\GetQuery");
			try
			{
				IOrderedQueryable<TEntity> query = client.CreateDocumentQuery<TEntity>(collection.SelfLink);

				return query;
			}
			finally
			{
				Logger.LogInfo("Getting Azure query... Done", "AzureDocumentDbDataStoreFactory\\GetQuery");
			}
		}

		protected virtual Database GetDatabase(DocumentClient client)
		{
			return AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbDatabaseName()).Result;
		}
	}
}