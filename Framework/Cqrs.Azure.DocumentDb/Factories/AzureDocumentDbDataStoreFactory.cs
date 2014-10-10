#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public AzureDocumentDbDataStoreFactory(IAzureDocumentDbDataStoreConnectionStringFactory azureDocumentDbDataStoreConnectionStringFactory)
		{
			AzureDocumentDbDataStoreConnectionStringFactory = azureDocumentDbDataStoreConnectionStringFactory;
		}

		protected virtual DocumentClient GetClient()
		{
			DocumentClient client = AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbConnectionString();

			return client;
		}

		protected virtual DocumentCollection GetCollection<TEntity>(DocumentClient client, Database database)
		{
			DocumentCollection collection = CreateOrReadCollection(client, database, typeof(TEntity).FullName).Result;

			return collection;
		}

		protected virtual IOrderedQueryable<TEntity> GetQuery<TEntity>(DocumentClient client, DocumentCollection collection)
		{
			IOrderedQueryable<TEntity> query = client.CreateDocumentQuery<TEntity>(collection.SelfLink);

			return query;
		}

		protected virtual Database GetDatabase(DocumentClient client)
		{
			return CreateOrReadDatabase(client, AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbDatabaseName()).Result;
		}

		protected async Task<Database> CreateOrReadDatabase(DocumentClient client, string databaseName)
		{
			IEnumerable<Database> query = client.CreateDatabaseQuery()
				.Where(database => database.Id == databaseName)
				.AsEnumerable();
			Database result = query.SingleOrDefault();
			return result ?? await client.CreateDatabaseAsync(new Database { Id = databaseName });
		}

		protected async Task<DocumentCollection> CreateOrReadCollection(DocumentClient client, Database database, string collectionName)
		{
			IEnumerable<DocumentCollection> query = client.CreateDocumentCollectionQuery(database.SelfLink)
				.Where(documentCollection => documentCollection.Id == collectionName)
				.AsEnumerable();
			DocumentCollection result = query.SingleOrDefault();
			return result ?? await client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collectionName });
		}

	}
}