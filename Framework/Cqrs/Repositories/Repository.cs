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
using cdmdotnet.Logging;
using Cqrs.Entities;
using Cqrs.DataStores;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	/// <summary>
	/// Provides basic repository methods for operations with an <see cref="IDataStore{TData}"/>.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TQueryBuilder">The <see cref="Type"/> of the <see cref="QueryBuilder{TQueryStrategy, TData}"/> that will be used to build queries.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data held in storage.</typeparam>
	public abstract class Repository<TQueryStrategy, TQueryBuilder, TData> : IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : Entity
	{
		/// <summary>
		/// Gets or sets the <see cref="Func{TResult}"/> that is used to create new instances of <see cref="IDataStore{TData}"/>.
		/// </summary>
		protected Func<IDataStore<TData>> CreateDataStoreFunction { get; private set; }

		/// <summary>
		/// Gets or sets the <typeparamref name="TQueryBuilder"/> that will be used to build queries.
		/// </summary>
		protected TQueryBuilder QueryBuilder { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ITelemetryHelper"/>.
		/// </summary>
		protected ITelemetryHelper TelemetryHelper { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="Repository{TQueryStrategy,TQueryBuilder,TData}"/>
		/// </summary>
		protected Repository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
		{
			CreateDataStoreFunction = createDataStoreFunction;
			QueryBuilder = queryBuilder;
			TelemetryHelper = new NullTelemetryHelper();
		}

		#region Implementation of IRepository<TData>

		/// <summary>
		/// Create the newly provided <paramref name="data"/> to storage.
		/// </summary>
		public virtual void Create(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Add(data);
		}

		/// <summary>
		/// Create the newly provided <paramref name="data"/> to storage.
		/// </summary>
		public virtual void Create(IEnumerable<TData> data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Add(data);
		}

		/// <summary>
		/// Builds and executes the provided <paramref name="singleResultQuery"/>.
		/// </summary>
		/// <param name="singleResultQuery">The <see cref="ISingleResultQuery{TQueryStrategy,TData}"/> to build and execute.</param>
		/// <param name="throwExceptionWhenNoQueryResults">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>
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

		/// <summary>
		/// Builds and executes the provided <paramref name="resultQuery"/>.
		/// </summary>
		/// <param name="resultQuery">The <see cref="ICollectionResultQuery{TQueryStrategy,TData}"/> to build and execute.</param>
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

		/// <summary>
		/// Update the provided <paramref name="data"/> in storage.
		/// </summary>
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

		/// <summary>
		/// Delete all contents (normally by use of a truncate operation) in storage.
		/// </summary>
		public virtual void DeleteAll()
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.RemoveAll();
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> from storage.
		/// </summary>
		public void Destroy(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Destroy(data);
		}

		/// <summary>
		/// Load the <typeparamref name="TData"/> from storage identified by the provided <paramref name="rsn"/>.
		/// </summary>
		/// <param name="rsn">The identifier if the <typeparamref name="TData"/> to load.</param>
		/// <param name="throwExceptionOnMissingEntity">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>
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

		/// <summary>
		/// Calls <see cref="CreateDataStoreFunction"/> passing the <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A function defining a filter if required.</param>
		protected virtual IQueryable<TData> CreateQueryable(Expression<Func<TData, bool>> predicate)
		{
			return CreateDataStoreFunction().Where(predicate);
		}
	}
}