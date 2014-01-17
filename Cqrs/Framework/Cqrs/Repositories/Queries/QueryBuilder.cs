using System;
using System.Linq;
using Cqrs.Config;
using Cqrs.Entities;
using Cqrs.DataStores;

namespace Cqrs.Repositories.Queries
{
	public abstract class QueryBuilder<TQueryStrategy, TData> : IQueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TData : Entity
	{
		protected IDataStore<TData> DataStore { get; private set; }

		protected IServiceLocator DependencyResolver { get; private set; }

		protected QueryBuilder(IDataStore<TData> dataStore, IServiceLocator dependencyResolver)
		{
			DataStore = dataStore;
			DependencyResolver = dependencyResolver;
		}

		#region Implementation of IQueryBuilder<UserQueryStrategy,User>

		public IQueryable<TData> CreateQueryable(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery)
		{
			return GeneratePredicate(singleResultQuery.QueryStrategy.QueryPredicate);
		}

		public IQueryable<TData> CreateQueryable(ICollectionResultQuery<TQueryStrategy, TData> collectionResultQuery)
		{
			return GeneratePredicate(collectionResultQuery.QueryStrategy.QueryPredicate);
		}

		#endregion

		protected IQueryable<TData> GeneratePredicate(IQueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null)
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
			throw new Exception();
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

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeAny<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TPermissionScope>(queryStrategy.WithPermissionScopeAny))
				return null;

			return OnGeneratePredicateWithPermissionScopeAny<TPermissionScope>(queryPredicate, leftHandQueryable);
		}

		protected abstract IQueryable<TData> OnGeneratePredicateWithPermissionScopeAny<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable);

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeUser<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TPermissionScope>(queryStrategy.WithPermissionScopeUser))
				return null;

			return OnGeneratePredicateWithPermissionScopeUser<TPermissionScope>(queryPredicate, leftHandQueryable);
		}

		protected abstract IQueryable<TData> OnGeneratePredicateWithPermissionScopeUser<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable);

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeCompany<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TPermissionScope>(queryStrategy.WithPermissionScopeCompany))
				return null;

			return OnGeneratePredicateWithPermissionScopeCompany<TPermissionScope>(queryPredicate, leftHandQueryable);
		}

		protected abstract IQueryable<TData> OnGeneratePredicateWithPermissionScopeCompany<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable);

		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeCompanyAndUser<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TPermissionScope>(queryStrategy.WithPermissionScopeCompanyAndUser))
				return null;

			return OnGeneratePredicateWithPermissionScopeCompanyAndUser<TPermissionScope>(queryPredicate, leftHandQueryable);
		}

		protected abstract IQueryable<TData> OnGeneratePredicateWithPermissionScopeCompanyAndUser<TPermissionScope>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable);

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
			return DependencyResolver.GetService<TQueryStrategy>();
		}
	}
}