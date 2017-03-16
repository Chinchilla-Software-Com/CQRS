#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Entities;
using Cqrs.DataStores;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	public abstract class Repository<TQueryStrategy, TQueryBuilder, TData> : IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : Entity
	{
		protected Func<IDataStore<TData>> CreateDataStoreFunction { get; private set; }

		protected TQueryBuilder QueryBuilder { get; private set; }

		protected ITelemetryHelper TelemetryHelper { get; set; }

		protected Repository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
		{
			CreateDataStoreFunction = createDataStoreFunction;
			QueryBuilder = queryBuilder;
			TelemetryHelper = new NullTelemetryHelper();
		}

		#region Implementation of IRepository<TData>

		public virtual void Create(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Add(data);
		}

		public virtual void Create(IEnumerable<TData> data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Add(data);
		}

		public virtual ISingleResultQuery<TQueryStrategy, TData> Retrieve(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery, bool throwExceptionWhenNoQueryResults = true)
		{
			// The .Select(i => i) is necessary due to inheritance
			// http://stackoverflow.com/questions/1021274/linq-to-sql-mapping-exception-when-using-abstract-base-classes
			IQueryable<TData> query = QueryBuilder.CreateQueryable(singleResultQuery).Select(i => i);

			IEnumerable<TData> result = query.AsEnumerable();
			int finalResultCount = 0;
			TData finalResult = throwExceptionWhenNoQueryResults
				? result.Single()
				: result.SingleOrDefault();
			if (finalResult != null)
				finalResultCount = 1;

			try
			{
				return new SingleResultQuery<TQueryStrategy, TData>
				{
					QueryStrategy = singleResultQuery.QueryStrategy,
					Result = finalResult
				};
			}
			finally
			{
				TelemetryHelper.TrackMetric(string.Format("Cqrs/Repository/Retrieve/Single/{0}", typeof(TData).Name), finalResultCount);

				// This is disabled until I can figure out a better way to handle disposing these... as it will most likely dispose the data store... and that's not cool.
				/*
				var disposable = result as IDisposable;
				if (disposable != null)
					disposable.Dispose();
				*/
			}
		}

		public virtual ICollectionResultQuery<TQueryStrategy, TData> Retrieve(ICollectionResultQuery<TQueryStrategy, TData> resultQuery)
		{
			// The .Select(i => i) is necessary due to inheritance
			// http://stackoverflow.com/questions/1021274/linq-to-sql-mapping-exception-when-using-abstract-base-classes
			IQueryable<TData> result = QueryBuilder.CreateQueryable(resultQuery).Select(i => i);

			IList<TData> finalResult = result.ToList();

			try
			{
				return new CollectionResultQuery<TQueryStrategy, TData>
				{
					QueryStrategy = resultQuery.QueryStrategy,
					Result = finalResult
				};
			}
			finally
			{
				TelemetryHelper.TrackMetric(string.Format("Cqrs/Repository/Retrieve/Collection/{0}", typeof(TData).Name), finalResult.Count);
				// This is disabled until I can figure out a better way to handle disposing these... as it will most likely dispose the data store... and that's not cool.
				/*
				var disposable = result as IDisposable;
				if (disposable != null)
					disposable.Dispose();
				*/
			}
		}

		public virtual void Update(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Update(data);
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public virtual void Delete(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Remove(data);
		}

		public virtual void DeleteAll()
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.RemoveAll();
		}

		public void Destroy(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Destroy(data);
		}

		public virtual TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			using (IDataStore<TData> dataStore = CreateDataStoreFunction())
			{
				IEnumerable<TData> query = dataStore
					// The .Select(i => i) is necessary due to inheritance
					// http://stackoverflow.com/questions/1021274/linq-to-sql-mapping-exception-when-using-abstract-base-classes
					.Select(i => i)
					.Where(entity => entity.Rsn == rsn)
					.AsEnumerable();

				int finalResultCount = 0;
				TData result = throwExceptionOnMissingEntity
					? query.Single()
					: query.SingleOrDefault();
				if (result != null)
					finalResultCount = 1;

				try
				{
					return result;
				}
				finally
				{
					TelemetryHelper.TrackMetric(string.Format("Cqrs/Repository/Load/{0}", typeof(TData).Name), finalResultCount);
				}
			}
		}

		#endregion

		protected virtual IQueryable<TData> CreateQueryable(Expression<Func<TData, bool>> predicate)
		{
			return CreateDataStoreFunction().Where(predicate);
		}
	}
}