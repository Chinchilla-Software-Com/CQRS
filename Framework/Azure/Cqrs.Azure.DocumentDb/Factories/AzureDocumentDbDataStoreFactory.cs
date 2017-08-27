#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using cdmdotnet.Logging;
using Cqrs.DataStores;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.Factories
{
	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> collections from Azure DocumentDB
	/// </summary>
	public class AzureDocumentDbDataStoreFactory
	{
		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbDataStoreConnectionStringFactory"/>.
		/// </summary>
		protected IAzureDocumentDbDataStoreConnectionStringFactory AzureDocumentDbDataStoreConnectionStringFactory { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbHelper"/>.
		/// </summary>
		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureDocumentDbDataStoreFactory"/>.
		/// </summary>
		public AzureDocumentDbDataStoreFactory(IAzureDocumentDbDataStoreConnectionStringFactory azureDocumentDbDataStoreConnectionStringFactory, IAzureDocumentDbHelper azureDocumentDbHelper, ILogger logger)
		{
			AzureDocumentDbDataStoreConnectionStringFactory = azureDocumentDbDataStoreConnectionStringFactory;
			AzureDocumentDbHelper = azureDocumentDbHelper;
			Logger = logger;
		}

		/// <summary>
		/// Get a <see cref="DocumentClient"/> from the <see cref="AzureDocumentDbDataStoreConnectionStringFactory"/>.
		/// </summary>
		protected virtual DocumentClient GetClient()
		{
			DocumentClient client = AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbConnectionClient();

			return client;
		}

		/// <summary>
		/// Get a <see cref="DocumentCollection"/> from the <see cref="AzureDocumentDbDataStoreConnectionStringFactory"/>.
		/// </summary>
		protected virtual DocumentCollection GetCollection<TEntity>(DocumentClient client, Database database)
		{
			string collectionName = string.Format(AzureDocumentDbDataStoreConnectionStringFactory.UseSingleCollectionForAllDataStores() ? "{0}" : "{0}_{1}", AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbCollectionName(), typeof(TEntity).FullName);
			DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName).Result;

			return collection;
		}

		/// <summary>
		/// Get a blank <see cref="IOrderedQueryable{TEntity}"/>.
		/// </summary>
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

		/// <summary>
		/// Get the <see cref="Database"/> from the <see cref="AzureDocumentDbDataStoreConnectionStringFactory"/>.
		/// </summary>
		protected virtual Database GetDatabase(DocumentClient client)
		{
			return AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbDataStoreConnectionStringFactory.GetAzureDocumentDbDatabaseName()).Result;
		}
	}
}