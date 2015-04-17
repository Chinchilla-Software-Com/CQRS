using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;

namespace Cqrs.Sql.DataStores
{
	public static class SqlDataStoreExtensions
	{
		/// <summary>
		/// Use this one
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
		/// Use this one
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