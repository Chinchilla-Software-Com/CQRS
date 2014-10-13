#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb
{
	public class GlobalAzureDocumentDbConnectionCache : IAzureDocumentDbConnectionCache
	{
		protected static IDictionary<string, object> Cache { get; private set; }

		static GlobalAzureDocumentDbConnectionCache()
		{
			Cache = new Dictionary<string, object>();
		}

		public bool TryGetClient(string key, out DocumentClient client)
		{
			try
			{
				object cacheResult;
				bool result = Cache.TryGetValue(key, out cacheResult);
				client = cacheResult as DocumentClient;
				return result && client != null;
			}
			catch
			{
			}
			client = null;
			return false;
		}

		public void SetClient(string key, DocumentClient client)
		{
			Cache[key] = client;
		}

		public bool TryGetDatabase(string key, out Database database)
		{
			try
			{
				object cacheResult;
				bool result = Cache.TryGetValue(key, out cacheResult);
				database = cacheResult as Database;
				return result && database != null;
			}
			catch
			{
			}
			database = null;
			return false;
		}

		public void SetDatabase(string key, Database database)
		{
			Cache[key] = database;
		}

		public bool TryGetDocumentCollection(string key, out DocumentCollection documentCollection)
		{
			try
			{
				object cacheResult;
				bool result = Cache.TryGetValue(key, out cacheResult);
				documentCollection = cacheResult as DocumentCollection;
				return result && documentCollection != null;
			}
			catch
			{
			}
			documentCollection = null;
			return false;
		}

		public void SetDocumentCollection(string key, DocumentCollection documentCollection)
		{
			Cache[key] = documentCollection;
		}
	}
}