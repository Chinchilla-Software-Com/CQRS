#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;
using Cqrs.Entities;
using Cqrs.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.DataStores
{
	public class AzureDocumentDbDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		protected DocumentClient AzureDocumentDbClient { get; private set; }

		protected Database AzureDocumentDbDatabase { get; private set; }

		protected DocumentCollection AzureDocumentDbCollection { get; private set; }

		protected IOrderedQueryable<TData> AzureDocumentDbQuery { get; private set; }

		protected ILog Logger { get; private set; }

		public AzureDocumentDbDataStore(ILog logger, DocumentClient azureDocumentDbClient, Database azureDocumentDbDatabase, DocumentCollection azureDocumentDbCollection, IOrderedQueryable<TData> azureDocumentDbQuery)
		{
			Logger = logger;
			AzureDocumentDbClient = azureDocumentDbClient;
			AzureDocumentDbDatabase = azureDocumentDbDatabase;
			AzureDocumentDbCollection = azureDocumentDbCollection;
			AzureDocumentDbQuery = azureDocumentDbQuery;
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<TData> GetEnumerator()
		{
			return AzureDocumentDbQuery.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of IQueryable

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </returns>
		public Expression Expression
		{
			get { return AzureDocumentDbQuery.AsQueryable().Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return AzureDocumentDbQuery.AsQueryable().ElementType; }
		}

		/// <summary>
		/// Gets the singleResultQuery provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return AzureDocumentDbQuery.AsQueryable().Provider; }
		}

		#endregion

		#region Implementation of IDataStore<TData>

		public void Add(TData data)
		{
			Logger.LogDebug("Adding data to the Azure database", "AzureDocumentDbDataStore\\Add");
			try
			{
				ResourceResponse<Document> result = AzureDocumentDbClient.CreateDocumentAsync((AzureDocumentDbCollection).SelfLink, data).Result;
				Logger.LogDebug(string.Format("Cost of adding data in the Azure database:r\n{0}", result), "AzureDocumentDbDataStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the Azure database... Done", "AzureDocumentDbDataStore\\Add");
			}
		}

		public void Add(IEnumerable<TData> data)
		{
			foreach (TData model in data)
			{
				Add(model);
			}
		}

		public void Remove(TData data)
		{
			data.IsLogicallyDeleted = true;
			Update(data);
		}

		public void RemoveAll()
		{
			ResourceResponse<DocumentCollection> result = AzureDocumentDbClient.DeleteDocumentCollectionAsync(AzureDocumentDbCollection.SelfLink, new RequestOptions()).Result;
		}

		public void Update(TData data)
		{
			Logger.LogDebug("Updating data in the Azure database", "AzureDocumentDbDataStore\\Update");
			try
			{
				Logger.LogDebug("Getting existing document from the Azure database", "AzureDocumentDbDataStore\\Update");
				Document documentToUpdate = AzureDocumentDbClient.CreateDocumentQuery(AzureDocumentDbCollection.DocumentsLink)
						.Where(d => d.Id == data.Rsn.ToString())
						.AsEnumerable()
						.Single();
				Logger.LogDebug("Replacing existing document in the Azure database", "AzureDocumentDbDataStore\\Update");
				ResourceResponse<Document> result = AzureDocumentDbClient.ReplaceDocumentAsync(documentToUpdate.SelfLink, data).Result;
				Logger.LogDebug(string.Format("Cost of replacing existing document in the Azure database:r\n{0}", result), "AzureDocumentDbDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Updating data in the Azure database... Done", "AzureDocumentDbDataStore\\Update");
			}
		}

		#endregion
	}
}