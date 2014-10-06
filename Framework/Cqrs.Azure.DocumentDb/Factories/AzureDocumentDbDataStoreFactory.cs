#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

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
			return GetClientAsync().Result;
		}

		protected virtual IOrderedQueryable<TEntity> GetCollection<TEntity>(DocumentClient client, Database database)
		{
			return GetCollectionAsync<TEntity>(client, database).Result;
		}

		protected virtual Database GetDatabase(DocumentClient client)
		{
			return GetDatabaseAsync(client).Result;
		}

		protected async Task<DocumentClient> GetClientAsync()
		{
			DocumentClient client = AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbConnectionString();

			return client;
		}

		protected async Task<Database> GetDatabaseAsync(DocumentClient client)
		{
			var database = new Database { Id = AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbDatabaseName() };
			database = await client.CreateDatabaseAsync(database);

			return database;
		}

		protected async Task<IOrderedQueryable<TEntity>> GetCollectionAsync<TEntity>(DocumentClient client, Database database)
		{
			var collection = new DocumentCollection { Id = typeof(TEntity).FullName };
			collection = await client.CreateDocumentCollectionAsync(database.SelfLink, collection);

			IOrderedQueryable<TEntity> query = client.CreateDocumentQuery<TEntity>(collection.SelfLink);

			return query;
		}
	}
}