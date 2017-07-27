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
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Sql.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> that uses EntityFramework to support complex data structures with SQL Server.s
	/// </summary>
	public class SqlDataStore<TEntity, TDbEntity> : IDataStore<TEntity>
		where TDbEntity : class, new()
	{
		/// <summary>
		/// Gets or sets the DataContext.
		/// </summary>
		internal DataContext DataContext { get; private set; }

		/// <summary>
		/// Gets or sets the readable Table
		/// </summary>
		internal Table<TDbEntity> DataTable { get; private set; }

		/// <summary>
		/// Gets or sets the EntityQuery
		/// </summary>
		internal IQueryable<TEntity> EntityQuery { get; private set; }

		/// <summary>
		/// Gets or sets the DbEntityQuery
		/// </summary>
		internal IQueryable<TDbEntity> DbEntityQuery { get; private set; }

		/// <summary>
		/// Gets or sets the ExpressionConverter
		/// </summary>
		internal IExpressionTreeConverter ExpressionConverter { get; set; }

		/// <summary>
		/// Instantiates a new instance of the <see cref="SqlDataStore{TEntity,TDbEntity}"/> class
		/// </summary>
		public SqlDataStore(IExpressionTreeConverter expressionConverter, DataContext dataContext)
		{
			ExpressionConverter = expressionConverter;

			DataContext = dataContext;
			DataTable = DataContext.GetTable<TDbEntity>();
			DbEntityQuery = DataTable;
			EntityQuery = new List<TEntity>().AsQueryable();

			switch (DataContext.Connection.State)
			{
				case ConnectionState.Closed:
				case ConnectionState.Broken:
					DataContext.Connection.Open();
					DataContext.Transaction = DataContext.Connection.BeginTransaction();
					break;
			}
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="SqlDataStore{TEntity,TDbEntity}"/> class
		/// </summary>
		internal SqlDataStore(DataContext dataContext, Table<TDbEntity> dataTable, IQueryable<TDbEntity> dbEntityQuery, IQueryable<TEntity> entityQuery)
		{
			DataContext = dataContext;
			DataTable = dataTable;
			DbEntityQuery = dbEntityQuery;
			EntityQuery = entityQuery;
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TDbEntity> GetEnumerator()
		{
			return DataTable.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
		{
			return EntityQuery.GetEnumerator();
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
			get { return EntityQuery.Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return EntityQuery.ElementType; }
		}

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return EntityQuery.Provider; }
		}

		#endregion

		#region Implementation of IDataStore<T>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public void Add(TEntity data)
		{
			var converted = Converters.ConvertTo<TDbEntity>(data);
			DataTable.InsertOnSubmit(converted);
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public void Add(IEnumerable<TEntity> data)
		{
			DataTable.InsertAllOnSubmit(data.Select(x => Converters.ConvertTo<TDbEntity>(x)));
		}

		/// <summary>
		/// Will NOT mark the <paramref name="data"/> as logically (or soft) deleted. This will destroy and delete the row in the SQL Server.
		/// </summary>
		public void Remove(TEntity data)
		{
			Destroy(data);
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public void Destroy(TEntity data)
		{
			var converted = Converters.ConvertTo<TDbEntity>(data);
			DataTable.DeleteOnSubmit(converted);
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public void RemoveAll()
		{
			IList<TDbEntity> all = DataTable.ToList();
			DataTable.DeleteAllOnSubmit(all);
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public void Update(TEntity data)
		{
			var converted = Converters.ConvertTo<TDbEntity>(data);
			DataTable.Attach(converted);
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			try
			{
				DataContext.SubmitChanges();
				DataContext.Transaction.Commit();
			}
			catch
			{
				DataContext.Transaction.Rollback();
			}
			finally
			{
				DataContext.Transaction.Dispose();
				DataContext.Dispose();
			}
		}

		#endregion
	}
}