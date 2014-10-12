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
using Cqrs.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb
{
	public class AzureDocumentDbHelper : IAzureDocumentDbHelper
	{
		protected ILog Logger { get; private set; }

		public AzureDocumentDbHelper(ILog logger)
		{
			Logger = logger;
		}

		public async Task<Database> CreateOrReadDatabase(DocumentClient client, string databaseName)
		{
			Logger.LogDebug("Getting Azure database", "AzureDocumentDbHelper\\CreateOrReadDatabase");
			try
			{
				IEnumerable<Database> query = client.CreateDatabaseQuery()
					.Where(database => database.Id == databaseName)
					.AsEnumerable();
				Logger.LogDebug("Checking if the database exists", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				Database result = query.SingleOrDefault();
				if (result != null)
					return result;
				Logger.LogDebug("Creating and returning a new database", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				return await client.CreateDatabaseAsync(new Database { Id = databaseName });
			}
			finally
			{
				Logger.LogDebug("Getting Azure database... Done", "AzureDocumentDbHelper\\CreateOrReadDatabase");
			}
		}

		public async Task<DocumentCollection> CreateOrReadCollection(DocumentClient client, Database database, string collectionName)
		{
			Logger.LogDebug("Getting Azure collection", "AzureDocumentDbHelper\\CreateOrReadCollection");
			try
			{
				IEnumerable<DocumentCollection> query = client.CreateDocumentCollectionQuery(database.SelfLink)
					.Where(documentCollection => documentCollection.Id == collectionName)
					.AsEnumerable();
				Logger.LogDebug("Checking if the collection exists", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				DocumentCollection result = query.SingleOrDefault();
				if (result != null)
					return result;
				Logger.LogDebug("Creating and returning a new collection", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				return await client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collectionName });
			}
			finally
			{
				Logger.LogDebug("Getting Azure collection... Done", "AzureDocumentDbHelper\\CreateOrReadCollection");
			}
		}

	}
}