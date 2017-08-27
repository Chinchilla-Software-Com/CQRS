#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.StateManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb
{
	/// <summary>
	/// A cache manager for DocumentDB clients, databases and collections that is thread based.
	/// </summary>
	public class ThreadedAzureDocumentDbConnectionCache : IAzureDocumentDbConnectionCache
	{
		/// <summary>
		/// Gets the <see cref="IContextItemCollection">cache</see> used.
		/// </summary>
		protected IContextItemCollection Cache { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="ThreadedAzureDocumentDbConnectionCache"/>.
		/// </summary>
		public ThreadedAzureDocumentDbConnectionCache(IContextItemCollectionFactory factory)
		{
			Cache = factory.GetCurrentContext();
		}

		/// <summary>
		/// Gets the <see cref="DocumentClient"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentClient"/> to get.</param>
		/// <param name="client">If the <see cref="DocumentClient"/> is found, it is returned here; otherwise null is returned. This parameter is passed uninitialized.</param>
		/// <returns>true if the  <see cref="DocumentClient"/> is found; otherwise, false.</returns>
		public bool TryGetClient(string key, out DocumentClient client)
		{
			try
			{
				var results = Cache.GetData<DocumentClient>(key);
				if (results != null)
				{
					client = results;
					return true;
				}
			}
			catch { /* */ }
			client = null;
			return false;
		}

		/// <summary>
		/// Sets the provided <paramref name="client"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentClient"/> to get.</param>
		/// <param name="client">The <see cref="DocumentClient"/> to set.</param>
		public void SetClient(string key, DocumentClient client)
		{
			Cache.SetData(key, client);
		}

		/// <summary>
		/// Gets the <see cref="Database"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="Database"/> to get.</param>
		/// <param name="database">If the <see cref="Database"/> is found, it is returned here; otherwise null is returned. This parameter is passed uninitialized.</param>
		/// <returns>true if the  <see cref="Database"/> is found; otherwise, false.</returns>
		public bool TryGetDatabase(string key, out Database database)
		{
			try
			{
				var results = Cache.GetData<Database>(key);
				if (results != null)
				{
					database = results;
					return true;
				}
			}
			catch { /* */ }
			database = null;
			return false;
		}

		/// <summary>
		/// Sets the provided <paramref name="database"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="Database"/> to get.</param>
		/// <param name="database">The <see cref="Database"/> to set.</param>
		public void SetDatabase(string key, Database database)
		{
			Cache.SetData(key, database);
		}

		/// <summary>
		/// Gets the <see cref="DocumentCollection"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentCollection"/> to get.</param>
		/// <param name="documentCollection">If the <see cref="DocumentCollection"/> is found, it is returned here; otherwise null is returned. This parameter is passed uninitialized.</param>
		/// <returns>true if the  <see cref="DocumentCollection"/> is found; otherwise, false.</returns>
		public bool TryGetDocumentCollection(string key, out DocumentCollection documentCollection)
		{
			try
			{
				var results = Cache.GetData<DocumentCollection>(key);
				if (results != null)
				{
					documentCollection = results;
					return true;
				}
			}
			catch { /* */ }
			documentCollection = null;
			return false;
		}

		/// <summary>
		/// Sets the provided <paramref name="documentCollection"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentCollection"/> to get.</param>
		/// <param name="documentCollection">The <see cref="DocumentCollection"/> to set.</param>
		public void SetDocumentCollection(string key, DocumentCollection documentCollection)
		{
			Cache.SetData(key, documentCollection);
		}
	}
}