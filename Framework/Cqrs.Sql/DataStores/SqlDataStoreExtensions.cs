#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;

namespace Cqrs.Sql.DataStores
{
	/// <summary>
	/// A collection of extension methods for <see cref="IDataStore{TData}"/>
	/// </summary>
	public static class SqlDataStoreExtensions
	{
		/// <summary>
		/// Use this one... Filters a sequence of values based on a predicate.
		/// </summary>
		public static IQueryable<TEntity> Where<TEntity, TDbEntity>(this IDataStore<TEntity> dataStore, Expression<Func<TEntity, bool>> predicate)
			where TDbEntity : class, new()
		{
			var sqlDataStore = dataStore as SqlDataStore<TEntity, TDbEntity>;
			if (sqlDataStore != null)
			{
				Expression expression = sqlDataStore.ExpressionConverter.Visit(predicate);

				return new SqlDataStore<TEntity, TDbEntity>
				(
					sqlDataStore.DataContext,
					sqlDataStore.DataTable,
					sqlDataStore.DbEntityQuery.Where((Expression<Func<TDbEntity, bool>>)expression),
					sqlDataStore.EntityQuery.Where(predicate)
				);
			}

			return dataStore.Where(predicate);
		}

		/// <summary>
		/// Use this one... Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
		/// </summary>
		public static TEntity Single<TEntity, TDbEntity>(this IEnumerable<TEntity> dataStore)
			where TDbEntity : class, new()
			where TEntity : new()
		{
			var sqlDataStore = dataStore as SqlDataStore<TEntity, TDbEntity>;
			if (sqlDataStore != null)
			{
				TDbEntity result = sqlDataStore.DbEntityQuery.Single();
				var convertedResult = Converters.ConvertTo<TEntity>(result);
				return convertedResult;
			}
			return dataStore.Single();
		}
	}
}