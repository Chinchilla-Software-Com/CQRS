﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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
using Chinchilla.Logging;
using Cqrs.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cqrs.Azure.DocumentDb.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> that uses Azure DocumentDB (CosmosDB) for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="IEntity"/> the <see cref="IDataStore{TData}"/> will contain.</typeparam>
	public class AzureDocumentDbDataStore<TData> : IDataStore<TData>
		where TData : AzureDocumentDbEntity
	{
		/// <summary>
		/// Gets or sets the <see cref="DocumentClient"/>
		/// </summary>
		protected DocumentClient AzureDocumentDbClient { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="Database"/>
		/// </summary>
		protected Database AzureDocumentDbDatabase { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="DocumentCollection"/>
		/// </summary>
		protected DocumentCollection AzureDocumentDbCollection { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IOrderedQueryable{T}"/>
		/// </summary>
		protected IOrderedQueryable<TData> AzureDocumentDbQuery { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbHelper"/>
		/// </summary>
		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates and Initialises a new instance of the <see cref="AzureDocumentDbDataStore{TData}"/> class.
		/// </summary>
		public AzureDocumentDbDataStore(ILogger logger, DocumentClient azureDocumentDbClient, Database azureDocumentDbDatabase, DocumentCollection azureDocumentDbCollection, IOrderedQueryable<TData> azureDocumentDbQuery, IAzureDocumentDbHelper azureDocumentDbHelper)
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
			Logger.LogDebug("Getting the enumerator for an Azure database query", "AzureDocumentDbDataStore\\GetEnumerator");
			try
			{
				DateTime start = DateTime.Now;
				IEnumerator<TData> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbQuery.GetEnumerator());
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Getting the enumerator for an Azure database query took {0}", end - start), "AzureDocumentDbDataStore\\GetEnumerator");
				return result;
			}
			finally
			{
				Logger.LogDebug("Getting the enumerator for an Azure database query... Done", "AzureDocumentDbDataStore\\GetEnumerator");
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
			get { return AzureDocumentDbQuery.Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return AzureDocumentDbQuery.ElementType; }
		}

		/// <summary>
		/// Gets the singleResultQuery provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return AzureDocumentDbQuery.Provider; }
		}

		#endregion

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public void Add(TData data)
		{
			Logger.LogDebug("Adding data to the Azure database", "AzureDocumentDbDataStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				ResourceResponse<Document> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.CreateDocumentAsync((AzureDocumentDbCollection).SelfLink, data).Result);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the Azure database took {0} and cost:r\n{1}", end - start, result), "AzureDocumentDbDataStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the Azure database... Done", "AzureDocumentDbDataStore\\Add");
			}
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public void Add(IEnumerable<TData> data)
		{
			Logger.LogDebug("Adding data collection to the Azure database", "AzureDocumentDbDataStore\\Add");
			try
			{
				foreach (TData model in data)
				{
					Add(model);
				}
			}
			finally
			{
				Logger.LogDebug("Adding data collection to the Azure database... Done", "AzureDocumentDbDataStore\\Add");
			}
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) by setting <see cref="Entity.IsDeleted"/> to true
		/// </summary>
		public void Remove(TData data)
		{
			Logger.LogDebug("Removing data from the Azure database", "AzureDocumentDbDataStore\\Remove");
			try
			{
				data.IsDeleted = true;
				Update(data);
			}
			finally
			{
				Logger.LogDebug("Removing data from the Azure database... Done", "AzureDocumentDbDataStore\\Remove");
			}
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public void Destroy(TData data)
		{
			Logger.LogDebug("Destroying data from the Azure database", "AzureDocumentDbDataStore\\Destroy");
			try
			{
				Logger.LogDebug("Getting existing document from the Azure database", "AzureDocumentDbDataStore\\Destroy");
				DateTime start = DateTime.Now;
				Document documentToUpdate = AzureDocumentDbClient.CreateDocumentQuery(AzureDocumentDbCollection.DocumentsLink)
						.Where(d => d.Id == data.id)
						.AsEnumerable()
						.Single();
				DateTime mid = DateTime.Now;
				Logger.LogDebug(string.Format("Getting existing document from the Azure database took {0}", mid - start), "AzureDocumentDbDataStore\\Destroy");
				Logger.LogDebug("Destroying existing document in the Azure database", "AzureDocumentDbDataStore\\Destroy");
				ResourceResponse<Document> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.DeleteDocumentAsync(documentToUpdate.SelfLink).Result);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Destroying existing document in the Azure database took {0} and cost:r\n{1}", end - mid, result), "AzureDocumentDbDataStore\\Destroy");
			}
			finally
			{
				Logger.LogDebug("Destroying data from the Azure database... Done", "AzureDocumentDbDataStore\\Destroy");
			}
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public void RemoveAll()
		{
			Logger.LogDebug("Removing all from the Azure database", "AzureDocumentDbDataStore\\RemoveAll");
			try
			{
				ResourceResponse<DocumentCollection> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.DeleteDocumentCollectionAsync(AzureDocumentDbCollection.SelfLink, new RequestOptions()).Result);
			}
			finally
			{
				Logger.LogDebug("Removing all from the Azure database... Done", "AzureDocumentDbDataStore\\RemoveAll");
			}
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public void Update(TData data)
		{
			Logger.LogDebug("Updating data in the Azure database", "AzureDocumentDbDataStore\\Update");
			try
			{
				Logger.LogDebug("Getting existing document from the Azure database", "AzureDocumentDbDataStore\\Update");
				DateTime start = DateTime.Now;
				Document documentToUpdate = AzureDocumentDbClient.CreateDocumentQuery(AzureDocumentDbCollection.DocumentsLink)
						.Where(d => d.Id == data.id)
						.AsEnumerable()
						.Single();
				DateTime mid = DateTime.Now;
				Logger.LogDebug(string.Format("Getting existing document from the Azure database took {0}", mid - start), "AzureDocumentDbDataStore\\Update");
				Logger.LogDebug("Replacing existing document in the Azure database", "AzureDocumentDbDataStore\\Update");
				ResourceResponse<Document> result = AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() => AzureDocumentDbClient.ReplaceDocumentAsync(documentToUpdate.SelfLink, data).Result);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Replacing existing document in the Azure database took {0} and cost:r\n{1}", end - mid, result), "AzureDocumentDbDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Updating data in the Azure database... Done", "AzureDocumentDbDataStore\\Update");
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

		/// <summary>
		/// Returns the instance as an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TData> AsEnumerable()
		{
			return AzureDocumentDbQuery.AsEnumerable();
		}
	}
}