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
using Cqrs.Azure.DocumentDb.Entities;
using Cqrs.DataStores;
using Cqrs.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.DataStores
{
	public class AzureDocumentDbDataStore<TData> : IDataStore<TData>
		where TData : AzureDocumentDbEntity
	{
		protected DocumentClient AzureDocumentDbClient { get; private set; }

		protected Database AzureDocumentDbDatabase { get; private set; }

		protected DocumentCollection AzureDocumentDbCollection { get; private set; }

		protected IOrderedQueryable<TData> AzureDocumentDbQuery { get; private set; }

		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		protected ILog Logger { get; private set; }

		public AzureDocumentDbDataStore(ILog logger, DocumentClient azureDocumentDbClient, Database azureDocumentDbDatabase, DocumentCollection azureDocumentDbCollection, IOrderedQueryable<TData> azureDocumentDbQuery, IAzureDocumentDbHelper azureDocumentDbHelper)
		{
			Logger = logger;
			AzureDocumentDbClient = azureDocumentDbClient;
			AzureDocumentDbDatabase = azureDocumentDbDatabase;
			AzureDocumentDbCollection = azureDocumentDbCollection;
			AzureDocumentDbQuery = azureDocumentDbQuery;
			AzureDocumentDbHelper = azureDocumentDbHelper;
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
			Logger.LogInfo("Getting the enumerator for an Azure database query", "AzureDocumentDbDataStore\\GetEnumerator");
			try
			{
				DateTime start = DateTime.Now;
				IEnumerator<TData> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbQuery.GetEnumerator());
				DateTime end = DateTime.Now;
				Logger.LogInfo(string.Format("Getting the enumerator for an Azure database query took {0}", end - start), "AzureDocumentDbDataStore\\GetEnumerator");
				return result;
			}
			finally
			{
				Logger.LogInfo("Getting the enumerator for an Azure database query... Done", "AzureDocumentDbDataStore\\GetEnumerator");
			}
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
			Logger.LogInfo("Adding data to the Azure database", "AzureDocumentDbDataStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				ResourceResponse<Document> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.CreateDocumentAsync((AzureDocumentDbCollection).SelfLink, data).Result);
				DateTime end = DateTime.Now;
				Logger.LogInfo(string.Format("Adding data in the Azure database took {0} and cost:r\n{1}", end - start, result), "AzureDocumentDbDataStore\\Add");
			}
			finally
			{
				Logger.LogInfo("Adding data to the Azure database... Done", "AzureDocumentDbDataStore\\Add");
			}
		}

		public void Add(IEnumerable<TData> data)
		{
			Logger.LogInfo("Adding data collection to the Azure database", "AzureDocumentDbDataStore\\Add");
			try
			{
				foreach (TData model in data)
				{
					Add(model);
				}
			}
			finally
			{
				Logger.LogInfo("Adding data collection to the Azure database... Done", "AzureDocumentDbDataStore\\Add");
			}
		}

		public void Remove(TData data)
		{
			Logger.LogInfo("Removing data from the Azure database", "AzureDocumentDbDataStore\\Remove");
			try
			{
				data.IsLogicallyDeleted = true;
				Update(data);
			}
			finally
			{
				Logger.LogInfo("Removing data from the Azure database... Done", "AzureDocumentDbDataStore\\Remove");
			}
		}

		public void RemoveAll()
		{
			Logger.LogInfo("Removing all from the Azure database", "AzureDocumentDbDataStore\\RemoveAll");
			try
			{
				ResourceResponse<DocumentCollection> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.DeleteDocumentCollectionAsync(AzureDocumentDbCollection.SelfLink, new RequestOptions()).Result);
			}
			finally
			{
				Logger.LogInfo("Removing all from the Azure database... Done", "AzureDocumentDbDataStore\\RemoveAll");
			}
		}

		public void Update(TData data)
		{
			Logger.LogInfo("Updating data in the Azure database", "AzureDocumentDbDataStore\\Update");
			try
			{
				Logger.LogInfo("Getting existing document from the Azure database", "AzureDocumentDbDataStore\\Update");
				DateTime start = DateTime.Now;
				Document documentToUpdate = AzureDocumentDbClient.CreateDocumentQuery(AzureDocumentDbCollection.DocumentsLink)
						.Where(d => d.Id == data.id)
						.AsEnumerable()
						.Single();
				DateTime mid = DateTime.Now;
				Logger.LogInfo(string.Format("Getting existing document from the Azure database took {0}", mid - start), "AzureDocumentDbDataStore\\Update");
				Logger.LogInfo("Replacing existing document in the Azure database", "AzureDocumentDbDataStore\\Update");
				ResourceResponse<Document> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.ReplaceDocumentAsync(documentToUpdate.SelfLink, data).Result);
				DateTime end = DateTime.Now;
				Logger.LogInfo(string.Format("Replacing existing document in the Azure database took {0} and cost:r\n{1}", end - mid, result), "AzureDocumentDbDataStore\\Update");
			}
			finally
			{
				Logger.LogInfo("Updating data in the Azure database... Done", "AzureDocumentDbDataStore\\Update");
			}
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			AzureDocumentDbClient.Dispose();
		}

		#endregion
	}
}