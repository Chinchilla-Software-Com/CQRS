#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Domain.Exceptions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.ObjectModel;

namespace Cqrs.Azure.DocumentDb
{
	/// <summary>
	/// A helper for Azure Document DB.
	/// </summary>
	public class AzureDocumentDbHelper : IAzureDocumentDbHelper
	{
		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets the <see cref="IAzureDocumentDbConnectionCache"/>
		/// </summary>
		protected IAzureDocumentDbConnectionCache AzureDocumentDbConnectionCache { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureDocumentDbHelper"/>.
		/// </summary>
		public AzureDocumentDbHelper(ILogger logger, IAzureDocumentDbConnectionCache azureDocumentDbConnectionCache)
		{
			Logger = logger;
			AzureDocumentDbConnectionCache = azureDocumentDbConnectionCache;
		}

		/// <summary>
		/// Gets a <see cref="Database"/> creating one if it doesn't already exist.
		/// </summary>
		/// <param name="client">The <see cref="DocumentClient"/> to use.</param>
		/// <param name="databaseName">The name of the database to get.</param>
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
					return await Task.FromResult(result);
				}
				finally
				{
					Logger.LogDebug("Returning cached database took... Done", "AzureDocumentDbHelper\\CreateOrReadDatabase");
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
						return await Task.FromResult(result);
					}
					finally
					{
						Logger.LogDebug("Returning the existing database... Done", "AzureDocumentDbHelper\\CreateOrReadDatabase");
					}
				}
				Logger.LogDebug("Creating and returning a new database", "AzureDocumentDbHelper\\CreateOrReadDatabase");
				start = DateTime.Now;

				result = ExecuteFaultTollerantFunction(() => client.CreateDatabaseAsync(new Database { Id = databaseName }).Result);

				Logger.LogDebug(string.Format("Getting Azure database took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadDatabase");
				AzureDocumentDbConnectionCache.SetDatabase(databaseCacheKey, result);
				return await Task.FromResult(result);
			}
			finally
			{
				Logger.LogDebug("Getting Azure database... Done", "AzureDocumentDbHelper\\CreateOrReadDatabase");
			}
		}

		/// <summary>
		/// Gets a <see cref="DocumentCollection"/> creating one if it doesn't already exist.
		/// </summary>
		/// <param name="client">The <see cref="DocumentClient"/> to use.</param>
		/// <param name="database">The <see cref="Database"/> to look in.</param>
		/// <param name="collectionName">The name of the collection to get.</param>
		/// <param name="uniqiueIndexPropertyNames">Any unique properties the collection should enforce.</param>
		public async Task<DocumentCollection> CreateOrReadCollection(DocumentClient client, Database database, string collectionName, string[] uniqiueIndexPropertyNames = null)
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
					return await Task.FromResult(result);
				}
				finally
				{
					Logger.LogDebug("Returning cached collection took... Done", "AzureDocumentDbHelper\\CreateOrReadCollection");
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
						// AzureDocumentDbConnectionCache.SetDocumentCollection(documentCollectionCacheKey, result);
						return await Task.FromResult(result);
					}
					finally
					{
						Logger.LogDebug("Returning the existing document collection... Done", "AzureDocumentDbHelper\\CreateOrReadCollection");
					}
				}
				Logger.LogDebug("Creating and returning a new collection", "AzureDocumentDbHelper\\CreateOrReadCollection");
				start = DateTime.Now;

				DocumentCollection myCollection = new DocumentCollection();
				myCollection.Id = collectionName;
				if (uniqiueIndexPropertyNames != null)
				{
					var paths = new Collection<string>{string.Format("/{0}", uniqiueIndexPropertyNames.First().Replace('.', '/'))};
					foreach (string name in uniqiueIndexPropertyNames.Skip(1))
						paths.Add(string.Format("/{0}", name.Replace('.', '/')));

					myCollection.UniqueKeyPolicy = new UniqueKeyPolicy
					{
						UniqueKeys = new Collection<UniqueKey>{new UniqueKey { Paths = paths } }
					};
				}

				result = ExecuteFaultTollerantFunction(() => client.CreateDocumentCollectionAsync(database.SelfLink, myCollection).Result);

				Logger.LogDebug(string.Format("Getting Azure document collection took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadCollection");
				// AzureDocumentDbConnectionCache.SetDocumentCollection(documentCollectionCacheKey, result);
				return await Task.FromResult(result);
			}
			finally
			{
				Logger.LogDebug("Getting Azure collection... Done", "AzureDocumentDbHelper\\CreateOrReadCollection");
			}
		}

		/// <summary>
		/// Process the provided <paramref name="documentClientException"/> checking for operations that can be retired.
		/// </summary>
		/// <param name="documentClientException">The <see cref="DocumentClientException"/> to check.</param>
		protected virtual void ProcessFaultTollerantExceptions(DocumentClientException documentClientException)
		{
			var statusCode = (int)documentClientException.StatusCode;
			if (statusCode == 429 || statusCode == 503)
				Thread.Sleep(documentClientException.RetryAfter);
			else
			{
				Logger.LogWarning("Non-fault tolerant exception raised via DocumentClientException.", "AzureDocumentDbDataStore\\ProcessFaultTollerantExceptions");
				if (documentClientException.Error.Message == "Resource with specified id or name already exists.")
					throw new DuplicateCreateCommandException(documentClientException);
				throw new DocumentDbException(documentClientException);
			}
		}

		/// <summary>
		/// Execute the provided <paramref name="func"/> in a fault tolerant way.
		/// </summary>
		/// <param name="func">The <see cref="Func{T}"/> to execute.</param>
		public virtual T ExecuteFaultTollerantFunction<T>(Func<T> func)
		{
			while (true)
			{
				try
				{
					return func();
				}
				catch (DocumentClientException documentClientException)
				{
					Logger.LogWarning("DocumentClientException thrown.", "AzureDocumentDbDataStore\\ExecuteFaultTollerantFunction");
					ProcessFaultTollerantExceptions(documentClientException);
				}
				catch (AggregateException aggregateException)
				{
					var documentClientException = aggregateException.InnerException as DocumentClientException;
					if (documentClientException != null)
					{
						Logger.LogWarning("DocumentClientException thrown via AggregateException.", "AzureDocumentDbDataStore\\ExecuteFaultTollerantFunction", documentClientException);
						ProcessFaultTollerantExceptions(documentClientException);
					}
					else
						Logger.LogWarning("Non DocumentClientException raised via AggregateException.", "AzureDocumentDbDataStore\\ExecuteFaultTollerantFunction", aggregateException);
				}
			}
		}

		/// <summary>
		/// Execute the provided <paramref name="func"/> in a fault tolerant way.
		/// </summary>
		/// <param name="func">The <see cref="Action"/> to execute.</param>
		public virtual void ExecuteFaultTollerantFunction(Action func)
		{
			while (true)
			{
				try
				{
					func();
					return;
				}
				catch (DocumentClientException documentClientException)
				{
					ProcessFaultTollerantExceptions(documentClientException);
				}
				catch (AggregateException aggregateException)
				{
					var documentClientException = aggregateException.InnerException as DocumentClientException;
					if (documentClientException != null)
					{
						Logger.LogWarning("DocumentClientException thrown via AggregateException.", "AzureDocumentDbDataStore\\ExecuteFaultTollerantFunction", documentClientException);
						ProcessFaultTollerantExceptions(documentClientException);
					}
					else
						Logger.LogWarning("Non DocumentClientException raised via AggregateException.", "AzureDocumentDbDataStore\\ExecuteFaultTollerantFunction", aggregateException);
				}
			}
		}
	}
}