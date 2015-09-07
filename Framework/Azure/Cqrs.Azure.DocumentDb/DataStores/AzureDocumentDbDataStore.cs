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
using cdmdotnet.Logging;
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

		protected ILogger Logger { get; private set; }

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

		public void Remove(TData data)
		{
			Logger.LogDebug("Removing data from the Azure database", "AzureDocumentDbDataStore\\Remove");
			try
			{
				data.IsLogicallyDeleted = true;
				Update(data);
			}
			finally
			{
				Logger.LogDebug("Removing data from the Azure database... Done", "AzureDocumentDbDataStore\\Remove");
			}
		}

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

namespace System.Linq
{
	using Cqrs.Azure.DocumentDb.DataStores;

	/// <summary>
	/// Provides a set of static (Shared in Visual Basic) methods for querying objects that inherit <see cref="AzureDocumentDbDataStore{TData}"/>.
	/// </summary>
	public static class AzureDocumentDbDataStoreExtensions
	{
		/// <summary>
		/// Returns the only element of the sequence, and throws an exception if there is not exactly one element in the sequence.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		/// <exception cref="InvalidOperationException">The sequence contains more than one element, or the sequence is empty.</exception>
		public static TData Single<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().Single();
		}

		/// <summary>
		/// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		/// <exception cref="InvalidOperationException">The sequence is empty.</exception>
		public static TData Single<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.Single();
		}

		/// <summary>
		/// Returns the only element of the sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		public static TData SingleOrDefault<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().SingleOrDefault();
		}

		/// <summary>
		/// Returns the only element of the sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		public static TData SingleOrDefault<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.SingleOrDefault();
		}

		/// <summary>
		/// Returns the first element of the sequence.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		/// <exception cref="InvalidOperationException">The sequence contains more than one element, or the sequence is empty.</exception>
		public static TData First<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().First();
		}

		/// <summary>
		/// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		/// <exception cref="InvalidOperationException">The sequence is empty.</exception>
		public static TData First<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.First();
		}

		/// <summary>
		/// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		public static TData FirstOrDefault<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().FirstOrDefault();
		}

		/// <summary>
		/// Returns the first element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		public static TData FirstOrDefault<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.FirstOrDefault();
		}
	}
}