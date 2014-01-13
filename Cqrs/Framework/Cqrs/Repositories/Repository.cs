using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.Entities;
using Cqrs.DataStores;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	public abstract class Repository<TQueryStrategy, TQueryBuilder, TData> : IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy, new()
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : Entity
	{
		protected IDataStore<TData> DataStore { get; private set; }

		protected TQueryBuilder QueryBuilder { get; private set; }

		protected Repository(IDataStore<TData> dataStore, TQueryBuilder queryBuilder)
		{
			DataStore = dataStore;
			QueryBuilder = queryBuilder;
		}

		#region Implementation of IRepository<TData>

		public void Create(TData data)
		{
			DataStore.Add(data);
		}

		public void Create(IEnumerable<TData> data)
		{
			DataStore.Add(data);
		}

		public virtual ISingleResultQuery<TQueryStrategy, TData> Retrieve(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery, bool throwExceptionWhenNoQueryResults = true)
		{
			IQueryable<TData> result = QueryBuilder.CreateQueryable(singleResultQuery);

			return new SingleResultQuery<TQueryStrategy, TData>
			{
				QueryStrategy = singleResultQuery.QueryStrategy,
				Result = throwExceptionWhenNoQueryResults
					? result.Single()
					: result.SingleOrDefault()
			};
		}

		public virtual ICollectionResultQuery<TQueryStrategy, TData> Retrieve(ICollectionResultQuery<TQueryStrategy, TData> resultQuery)
		{
			IQueryable<TData> result = QueryBuilder.CreateQueryable(resultQuery);

			return new CollectionResultQuery<TQueryStrategy, TData>
			{
				QueryStrategy = resultQuery.QueryStrategy,
				Result = result.ToList()
			};
		}

		public void Update(TData data)
		{
			DataStore.Update(data);
		}

		public void Delete(TData data)
		{
			DataStore.Remove(data);
		}

		public void DeleteAll()
		{
			DataStore.RemoveAll();
		}

		public TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			if (throwExceptionOnMissingEntity)
				return DataStore.Single(entity => entity.Rsn == rsn);
			return DataStore.SingleOrDefault(entity => entity.Rsn == rsn);
		}

		#endregion

		protected IQueryable<TData> CreateQueryable(Expression<Func<TData, bool>> predicate)
		{
			return DataStore.Where(predicate);
		}

	}
}