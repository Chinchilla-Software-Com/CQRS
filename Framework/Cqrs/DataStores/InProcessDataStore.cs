#region Copyright
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
using Cqrs.Entities;
using Cqrs.Repositories;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> using an <see cref="InMemoryDatabase"/>.
	/// </summary>
	public class InProcessDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		private InMemoryDatabase InMemoryDatabase { get; set; }

		/// <summary>
		/// Instantiates a new instance of the <see cref="InProcessDataStore{TData}"/> class
		/// </summary>
		public InProcessDataStore()
		{
			InMemoryDatabase = new InMemoryDatabase();
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TData> GetEnumerator()
		{
			return InMemoryDatabase.GetAll<TData>().GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
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
			get { return InMemoryDatabase.GetAll<TData>().AsQueryable().Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return InMemoryDatabase.GetAll<TData>().AsQueryable().ElementType; }
		}

		/// <summary>
		/// Gets the singleResultQuery provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return InMemoryDatabase.GetAll<TData>().AsQueryable().Provider; }
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}

		#endregion

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public void Add(TData data)
		{
			InMemoryDatabase.Get<TData>().Add(data.Rsn, data);
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public void Add(IEnumerable<TData> data)
		{
			foreach (TData dataItem in data)
				Add(dataItem);
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) deleted by setting <see cref="Entity.IsDeleted"/> to true in the data store and persist the change.
		/// </summary>
		public void Remove(TData data)
		{
			InMemoryDatabase.Get<TData>()[data.Rsn].IsDeleted = true;
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public void Destroy(TData data)
		{
			InMemoryDatabase.Get<TData>().Remove(data.Rsn);
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public void RemoveAll()
		{
			InMemoryDatabase.Get<TData>().Clear();
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public void Update(TData data)
		{
			InMemoryDatabase.Get<TData>()[data.Rsn] = data;
		}

		#endregion
	}
}