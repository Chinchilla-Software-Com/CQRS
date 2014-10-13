#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
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

		protected IAzureDocumentDbConnectionCache AzureDocumentDbConnectionCache { get; private set; }

		public AzureDocumentDbHelper(ILog logger, IAzureDocumentDbConnectionCache azureDocumentDbConnectionCache)
		{
			Logger = logger;
			AzureDocumentDbConnectionCache = azureDocumentDbConnectionCache;
		}

		public async Task<Database> CreateOrReadDatabase(DocumentClient client, string databaseName)
		{
			Logger.LogDebug("Getting Azure database", "AzureDocumentDbHelper\\CreateOrReadDatabase");
			DateTime start = DateTime.Now;
			Database result;
			string databaseCacheKey = string.Format("AzureDocumentDbDatabase::{0}", databaseName);
			if (AzureDocumentDbConnectionCache.TryGetDatabase(databaseCacheKey, out result))
			{
				Logger.LogDebug(string.Format("Returning cached database took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadDatabase");
				try
				{
					return result;
				}
				finally
				{
					Logger.LogDebug(string.Format("Returning cached database took... Done"), "AzureDocumentDbHelper\\CreateOrReadDatabase");
				}
			}
			try
			{
				IEnumerable<Database> query = client.CreateDatabaseQuery()
					.Where(database => database.Id == databaseName)
					.AsEnumerable();
				Logger.LogDebug("Checking if the database exists", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				start = DateTime.Now;
				result = query.SingleOrDefault();
				if (result != null)
				{
					Logger.LogDebug(string.Format("Returning the existing database took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadDatabase");
					try
					{
						AzureDocumentDbConnectionCache.SetDatabase(databaseCacheKey, result);
						return result;
					}
					finally
					{
						Logger.LogDebug(string.Format("Returning the existing database... Done"), "AzureDocumentDbHelper\\CreateOrReadDatabase");
					}
				}
				Logger.LogDebug("Creating and returning a new database", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				start = DateTime.Now;
				result = client.CreateDatabaseAsync(new Database { Id = databaseName }).Result;
				Logger.LogDebug(string.Format("Getting Azure database took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadDatabase");
				AzureDocumentDbConnectionCache.SetDatabase(databaseCacheKey, result);
				return result;
			}
			finally
			{
				Logger.LogDebug("Getting Azure database... Done", "AzureDocumentDbHelper\\CreateOrReadDatabase");
			}
		}

		public async Task<DocumentCollection> CreateOrReadCollection(DocumentClient client, Database database, string collectionName)
		{
			Logger.LogDebug("Getting Azure collection", "AzureDocumentDbHelper\\CreateOrReadCollection");
			DateTime start = DateTime.Now;
			DocumentCollection result;
			string documentCollectionCacheKey = string.Format("AzureDocumentDbDocumentCollection::{0}", collectionName);
			if (AzureDocumentDbConnectionCache.TryGetDocumentCollection(documentCollectionCacheKey, out result))
			{
				Logger.LogDebug(string.Format("Returning cached collection took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadCollection");
				try
				{
					return result;
				}
				finally
				{
					Logger.LogDebug(string.Format("Returning cached collection took... Done"), "AzureDocumentDbHelper\\CreateOrReadCollection");
				}
			}
			try
			{
				IEnumerable<DocumentCollection> query = client.CreateDocumentCollectionQuery(database.SelfLink)
					.Where(documentCollection => documentCollection.Id == collectionName)
					.AsEnumerable();
				Logger.LogDebug("Checking if the collection exists", "AzureDocumentDbHelper\\CreateOrReadCollection");
				start = DateTime.Now;
				result = query.SingleOrDefault();
				if (result != null)
				{
					Logger.LogDebug(string.Format("Returning the existing document collection took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadCollection");
					try
					{
						AzureDocumentDbConnectionCache.SetDocumentCollection(documentCollectionCacheKey, result);
						return result;
					}
					finally
					{
						Logger.LogDebug(string.Format("Returning the existing document collection... Done"), "AzureDocumentDbHelper\\CreateOrReadCollection");
					}
				}
				Logger.LogDebug("Creating and returning a new collection", "AzureDocumentDbHelper\\CreateOrReadCollection");
				start = DateTime.Now;
				result = client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collectionName }).Result;
				Logger.LogDebug(string.Format("Getting Azure document collection took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadCollection");
				AzureDocumentDbConnectionCache.SetDocumentCollection(documentCollectionCacheKey, result);
				return result;
			}
			finally
			{
				Logger.LogDebug("Getting Azure collection... Done", "AzureDocumentDbHelper\\CreateOrReadCollection");
			}
		}
	}
}