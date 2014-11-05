using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Cqrs.Entities;

namespace Cqrs.Mongo.DataStores
{
	public class MongoDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		protected MongoCollection<TData> MongoCollection { get; private set; }

		public MongoDataStore(MongoCollection<TData> mongoCollection)
		{
			MongoCollection = mongoCollection;
			MongoCollection.Database.RequestStart();
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
			return MongoCollection.AsQueryable().GetEnumerator();
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
			get { return MongoCollection.AsQueryable().Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return MongoCollection.AsQueryable().ElementType; }
		}

		/// <summary>
		/// Gets the singleResultQuery provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return MongoCollection.AsQueryable().Provider; }
		}

		#endregion

		#region Implementation of IDataStore<TData>

		public virtual void Add(TData data)
		{
			MongoCollection.Insert(data);
		}

		public virtual void Add(IEnumerable<TData> data)
		{
			MongoCollection.InsertBatch(data);
		}

		public virtual void Remove(TData data)
		{
			data.IsLogicallyDeleted = true;
			Update(data);
		}

		public virtual void RemoveAll()
		{
			MongoCollection.RemoveAll();
		}

		public virtual void Update(TData data)
		{
			MongoCollection.Save(data);
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			MongoCollection.Database.RequestDone();
		}

		#endregion
	}
}