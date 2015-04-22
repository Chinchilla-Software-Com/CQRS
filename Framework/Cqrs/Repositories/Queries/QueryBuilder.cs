using System;
using System.Linq;
using Cqrs.Configuration;
using Cqrs.Entities;
using Cqrs.DataStores;

namespace Cqrs.Repositories.Queries
{
	public abstract class QueryBuilder<TQueryStrategy, TData> : IQueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TData : Entity
	{
		protected IDataStore<TData> DataStore { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected QueryBuilder(IDataStore<TData> dataStore, IDependencyResolver dependencyResolver)
		{
			DataStore = dataStore;
			DependencyResolver = dependencyResolver;
		}

		#region Implementation of IQueryBuilder<UserQueryStrategy,User>

		public virtual IQueryable<TData> CreateQueryable(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery)
		{
			IQueryable<TData> queryable = singleResultQuery.QueryStrategy.QueryPredicate == null ?  GetEmptyQueryPredicate() : GeneratePredicate(singleResultQuery.QueryStrategy.QueryPredicate);
			ApplySorting(singleResultQuery.QueryStrategy, ref queryable);
			return queryable;
		}

		public virtual IQueryable<TData> CreateQueryable(ICollectionResultQuery<TQueryStrategy, TData> collectionResultQuery)
		{
			IQueryable<TData> queryable = collectionResultQuery.QueryStrategy.QueryPredicate == null ? GetEmptyQueryPredicate() : GeneratePredicate(collectionResultQuery.QueryStrategy.QueryPredicate);
			ApplySorting(collectionResultQuery.QueryStrategy, ref queryable);
			return queryable;
		}

		#endregion

		protected virtual IQueryable<TData> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		protected virtual IQueryable<TData> GeneratePredicate(IQueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null)
		{
			var andQueryPredicate = queryPredicate as IAndQueryPredicate;
			if (andQueryPredicate != null)
			{
				IQueryable<TData> innerLeftHandQueryable = GeneratePredicate(andQueryPredicate.LeftQueryPredicate);
				return GeneratePredicate(andQueryPredicate.RightQueryPredicate, innerLeftHandQueryable);
			}
			var orQueryPredicate = queryPredicate as IOrQueryPredicate;
			if (orQueryPredicate != null)
			{
				IQueryable<TData> innerLeftHandQueryable = GeneratePredicate(orQueryPredicate.LeftQueryPredicate);
				return GeneratePredicate(orQueryPredicate.RightQueryPredicate, innerLeftHandQueryable);
			}
			var realQueryPredicate = queryPredicate as QueryPredicate;
			if (realQueryPredicate != null)
			{
				return GeneratePredicateIsNotLogicallyDeleted(realQueryPredicate, leftHandQueryable) ?? GeneratePredicate(realQueryPredicate, leftHandQueryable);
			}
			throw new InvalidOperationException(string.Format("The query predicate '{0}' is unable to be processed.", queryPredicate == null ? typeof(void) : queryPredicate.GetType()));
		}

		protected virtual IQueryable<TData> GeneratePredicateIsNotLogicallyDeleted(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionName(queryStrategy.IsNotLogicallyDeleted))
				return null;

			return (leftHandQueryable ?? DataStore).Where
			(
				entity => !entity.IsLogicallyDeleted
			);
		}

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeAny<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeAny))
				return null;

			return OnGeneratePredicateWithPermissionScopeAny<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeAny<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? DataStore);
		}

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeUser))
				return null;

			return OnGeneratePredicateWithPermissionScopeUser<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? DataStore);
		}

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeCompany<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeCompany))
				return null;

			return OnGeneratePredicateWithPermissionScopeCompany<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeCompany<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? DataStore);
		}

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeCompanyAndUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeCompanyAndUser))
				return null;

			return OnGeneratePredicateWithPermissionScopeCompanyAndUser<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeCompanyAndUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? DataStore);
		}

		protected abstract IQueryable<TData> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null);

		protected virtual string GetFunctionNameOfType<TParameter1>(Func<TParameter1, IQueryPredicate> expression)
		{
			return expression.Method.Name;
		}

		protected virtual string GetFunctionName<T>(Func<T> expression)
		{
			return expression.Method.Name;
		}

		protected virtual string GetFunctionName<TParameter1>(Func<TParameter1, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		protected virtual string GetFunctionName<TParameter1, TParameter2>(Func<TParameter1, TParameter2, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		protected virtual string GetFunctionName<TParameter1, TParameter2, TParameter3>(Func<TParameter1, TParameter2, TParameter3, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		protected virtual string GetFunctionName<TParameter1, TParameter2, TParameter3, TParameter4>(Func<TParameter1, TParameter2, TParameter3, TParameter4, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		protected virtual string GetFunctionName<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5>(Func<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		protected virtual TQueryStrategy GetNullQueryStrategy()
		{
			return DependencyResolver.Resolve<TQueryStrategy>();
		}

		protected virtual void ApplySorting(TQueryStrategy queryStrategy, ref IQueryable<TData> queryable)
		{
		}
	}
}