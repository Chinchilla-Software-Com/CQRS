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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Domain.Exceptions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb
{
	public class AzureDocumentDbHelper : IAzureDocumentDbHelper
	{
		protected ILogger Logger { get; private set; }

		protected IAzureDocumentDbConnectionCache AzureDocumentDbConnectionCache { get; private set; }

		public AzureDocumentDbHelper(ILogger logger, IAzureDocumentDbConnectionCache azureDocumentDbConnectionCache)
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

				result = ExecuteFaultTollerantFunction(() => client.CreateDatabaseAsync(new Database { Id = databaseName }).Result);

				Logger.LogDebug(string.Format("Getting Azure database took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadDatabase");
				AzureDocumentDbConnectionCache.SetDatabase(databaseCacheKey, result);
				return result;
			}
			finally
			{
				Logger.LogDebug("Getting Azure database... Done", "AzureDocumentDbHelper\\CreateOrReadDatabase");
			}
		}

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
						// AzureDocumentDbConnectionCache.SetDocumentCollection(documentCollectionCacheKey, result);
						return result;
					}
					finally
					{
						Logger.LogDebug(string.Format("Returning the existing document collection... Done"), "AzureDocumentDbHelper\\CreateOrReadCollection");
					}
				}
				Logger.LogDebug("Creating and returning a new collection", "AzureDocumentDbHelper\\CreateOrReadCollection");
				start = DateTime.Now;
				result = ExecuteFaultTollerantFunction(() => client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collectionName }).Result);
				if (uniqiueIndexPropertyNames != null)
				{
					StringBuilder body = new StringBuilder(@"
function()
{
	var context = getContext();
	var collection = context.getCollection();
	var request = context.getRequest();

	// document to be created in the current operation
	var documentToCreate = request.getBody();

	function lookForDuplicates(propertyNames, propertyValues, continuation)
	{
		var queryString = 'SELECT * FROM c WHERE ';
		var queryParameters = [];
		for (index = 0; index < propertyNames.length; index++)
		{
			if (index > 0)
				queryString = queryString + ' AND';
			queryString = queryString + ' c.' + propertyNames[index] + ' = @property' + index;
			queryParameters.push({ name: '@property' + index, value: propertyValues[index] });
		}
		var query =
		{
			query: queryString,
			parameters: queryParameters
		};
		var requestOptions =
		{
			continuation: continuation
		};

		var isAccepted = collection.queryDocuments(collection.getSelfLink(), query, requestOptions,
			function(err, results, responseOptions)
			{
				if (err)
				{
					throw new Error('Error querying for documents with duplicate: ' + err.message);
				}
				if (results.length > 0)
				{
					// At least one document with property exists.
					throw new Error('Document with the property: ' + JSON.stringify(propertyNames) + ' and value: ' + JSON.stringify(propertyValues) + ', already exists: ' + JSON.stringify(results[0]));
				}
				else if (responseOptions.continuation)
				{
					// Else if the query came back empty, but with a continuation token; repeat the query w/ the token.
					// This is highly unlikely; but is included to serve as an example for larger queries.
					lookForDuplicates(propertyNames, propertyValues, responseOptions.continuation);
				}
				else
				{
					// Success, no duplicates found! Do nothing.
				}
			}
		);

		// If we hit execution bounds - throw an exception.
		// This is highly unlikely; but is included to serve as an example for more complex operations.
		if (!isAccepted)
		{
			throw new Error('Timeout querying for document with duplicates.');
		}
	}

");
					string propertyNames = uniqiueIndexPropertyNames.Aggregate("", (current, uniqiueIndexPropertyName) => string.Format("{0}{1}\"{2}\"", current, string.IsNullOrWhiteSpace(current) ? string.Empty : ", ", uniqiueIndexPropertyName));
					string propertyValues = uniqiueIndexPropertyNames.Aggregate("", (current, uniqiueIndexPropertyName) => string.Format("{0}{1}documentToCreate[\"{2}\"]", current, string.IsNullOrWhiteSpace(current) ? string.Empty : ", ", uniqiueIndexPropertyName));
					foreach (string uniqiueIndexPropertyName in uniqiueIndexPropertyNames)
					{
						/*
						if (uniqiueIndexPropertyName.Contains("::"))
						{
							string[] values = uniqiueIndexPropertyName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
							string preFilter = null;
							string propertyName = values[0];
							string subPropertyName = values[1];
							if (values.Length == 3)
							{
								preFilter = values[0];
								propertyName = values[1];
								subPropertyName = values[2];
							}
							body.Append(@"
	if (!(""" + propertyName + @""" in documentToCreate)) {
		throw new Error('Document must include a """ + propertyName + @""" property.');
	}
	// get property
	var propertyData = documentToCreate[""" + propertyName + @"""];
	var property = JSON.parse(propertyData);
");
						}
						else
						{
							body.Append(@"
	if (!(""" + uniqiueIndexPropertyName + @""" in documentToCreate)) {
		throw new Error('Document must include a """ + uniqiueIndexPropertyName + @""" property.');
	lookForDuplicates(""" + uniqiueIndexPropertyName + @""", documentToCreate[""" + uniqiueIndexPropertyName + @"""]);");
						}
						*/
							body.Append(@"
	if (!(""" + uniqiueIndexPropertyName + @""" in documentToCreate))
		throw new Error('Document must include a """ + uniqiueIndexPropertyName + @""" property.');");
					}

					body.Append(@"
	lookForDuplicates([" + propertyNames + @"], [" + propertyValues + @"]);
}");

					var trigger = new Trigger
					{
						Id = "ValidateUniqueConstraints",
						Body = body.ToString(),
						TriggerOperation = TriggerOperation.Create,
						TriggerType = TriggerType.Pre
					};
					ExecuteFaultTollerantFunction(() => client.CreateTriggerAsync(result.SelfLink, trigger).Result);
				}
				Logger.LogDebug(string.Format("Getting Azure document collection took {0}", DateTime.Now - start), "AzureDocumentDbHelper\\CreateOrReadCollection");
				// AzureDocumentDbConnectionCache.SetDocumentCollection(documentCollectionCacheKey, result);
				return result;
			}
			finally
			{
				Logger.LogDebug("Getting Azure collection... Done", "AzureDocumentDbHelper\\CreateOrReadCollection");
			}
		}

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
				throw new DocumentDbException("Non-fault tolerant exception raised.", documentClientException);
			}
		}

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