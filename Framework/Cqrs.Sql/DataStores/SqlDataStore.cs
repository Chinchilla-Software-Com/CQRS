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
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;

namespace Cqrs.Sql.DataStores
{
	public class SqlDataStore<TEntity, TDbEntity> : IDataStore<TEntity>
		where TDbEntity : class, new()
	{
		internal LinqToSqlDataContext DataContext { get; private set; }

		internal Table<TDbEntity> DataTable { get; private set; }

		internal IQueryable<TEntity> EntityQuery { get; private set; }

		internal IQueryable<TDbEntity> DbEntityQuery { get; private set; }

		internal IExpressionTreeConverter ExpressionConverter { get; set; }

		public SqlDataStore(IExpressionTreeConverter expressionConverter, LinqToSqlDataContext dataContext)
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

		internal SqlDataStore(LinqToSqlDataContext dataContext, Table<TDbEntity> dataTable, IQueryable<TDbEntity> dbEntityQuery, IQueryable<TEntity> entityQuery)
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

		public void Add(TEntity data)
		{
			var converted = Converters.ConvertTo<TDbEntity>(data);
			DataTable.InsertOnSubmit(converted);
		}

		public void Add(IEnumerable<TEntity> data)
		{
			DataTable.InsertAllOnSubmit(data.Select(x => Converters.ConvertTo<TDbEntity>(x)));
		}

		public void Remove(TEntity data)
		{
			var converted = Converters.ConvertTo<TDbEntity>(data);
			DataTable.DeleteOnSubmit(converted);
		}

		public void RemoveAll()
		{
			IList<TDbEntity> all = DataTable.ToList();
			DataTable.DeleteAllOnSubmit(all);
		}

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