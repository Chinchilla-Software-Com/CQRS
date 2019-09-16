#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Configuration;
using Cqrs.Entities;
using Cqrs.DataStores;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// Builds an <see cref="IQueryable"/> from a <typeparamref name="TQueryStrategy"/>.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of the <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data to query.</typeparam>
	public abstract class QueryBuilder<TQueryStrategy, TData> : IQueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TData : Entity
	{
		/// <summary>
		/// Gets or set the <see cref="IDataStore{TData}"/> to use.
		/// </summary>
		protected IDataStore<TData> DataStore { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="IDependencyResolver"/>.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="QueryBuilder{TQueryStrategy,TData}"/>.
		/// </summary>
		protected QueryBuilder(IDataStore<TData> dataStore, IDependencyResolver dependencyResolver)
		{
			DataStore = dataStore;
			DependencyResolver = dependencyResolver;
		}

		#region Implementation of IQueryBuilder<UserQueryStrategy,User>

		/// <summary>
		/// Create an <see cref="IQueryable"/> of <typeparamref name="TData"/>
		/// that expects a single <typeparamref name="TData"/> item.
		/// </summary>
		/// <param name="singleResultQuery">The query.</param>
		public virtual IQueryable<TData> CreateQueryable(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery)
		{
			IQueryable<TData> queryable = singleResultQuery.QueryStrategy.QueryPredicate == null ?  GetEmptyQueryPredicate() : GeneratePredicate(singleResultQuery.QueryStrategy.QueryPredicate);
			ApplySorting(singleResultQuery.QueryStrategy, ref queryable);
			return queryable;
		}

		/// <summary>
		/// Create an <see cref="IQueryable"/> of <typeparamref name="TData"/>
		/// that expects a collection of <typeparamref name="TData"/> items.
		/// </summary>
		/// <param name="collectionResultQuery">The query.</param>
		public virtual IQueryable<TData> CreateQueryable(ICollectionResultQuery<TQueryStrategy, TData> collectionResultQuery)
		{
			IQueryable<TData> queryable = collectionResultQuery.QueryStrategy.QueryPredicate == null ? GetEmptyQueryPredicate() : GeneratePredicate(collectionResultQuery.QueryStrategy.QueryPredicate);
			ApplySorting(collectionResultQuery.QueryStrategy, ref queryable);
			return queryable;
		}

		#endregion

		/// <summary>
		/// Returns the <see cref="DataStore"/> itself.
		/// </summary>
		protected virtual IQueryable<TData> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		/// <summary>
		/// Builds an <see cref="IQueryable"/> from the <paramref name="queryPredicate"/> and an optional <paramref name="leftHandQueryable"/>.
		/// This recursively calls itself and may call <see cref="GeneratePredicateIsNotLogicallyDeleted"/>.
		/// </summary>
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
				IQueryable<TData> result = GeneratePredicateIsNotLogicallyDeleted(realQueryPredicate, leftHandQueryable);
				return result ?? GeneratePredicate(realQueryPredicate, leftHandQueryable);
			}
			throw new InvalidOperationException(string.Format("The query predicate '{0}' is unable to be processed.", queryPredicate == null ? typeof(void) : queryPredicate.GetType()));
		}

		/// <summary>
		/// Builds the relevant <see cref="IQueryable"/> for <see cref="QueryStrategy.IsNotLogicallyDeleted"/>.
		/// </summary>
		protected virtual IQueryable<TData> GeneratePredicateIsNotLogicallyDeleted(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionName(queryStrategy.IsNotLogicallyDeleted))
				return null;

			return (leftHandQueryable ?? GetEmptyQueryPredicate()).Where
			(
				entity => !entity.IsLogicallyDeleted
			);
		}

		/// <summary>
		/// Builds the relevant <see cref="IQueryable"/> for <see cref="QueryStrategy.WithPermissionScopeAny{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeAny<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeAny))
				return null;

			return OnGeneratePredicateWithPermissionScopeAny<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		/// <summary>
		/// Returns <paramref name="leftHandQueryable"/> or calls <see cref="GetEmptyQueryPredicate"/> if <paramref name="leftHandQueryable"/> is null.
		/// Override to build the relevant permission scope <see cref="IQueryable"/>.
		/// </summary>
		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeAny<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? GetEmptyQueryPredicate());
		}

		/// <summary>
		/// Builds the relevant <see cref="IQueryable"/> for <see cref="QueryStrategy.WithPermissionScopeUser{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeUser))
				return null;

			return OnGeneratePredicateWithPermissionScopeUser<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		/// <summary>
		/// Returns <paramref name="leftHandQueryable"/> or calls <see cref="GetEmptyQueryPredicate"/> if <paramref name="leftHandQueryable"/> is null.
		/// Override to build the relevant permission scope <see cref="IQueryable"/>.
		/// </summary>
		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? GetEmptyQueryPredicate());
		}

		/// <summary>
		/// Builds the relevant <see cref="IQueryable"/> for <see cref="QueryStrategy.WithPermissionScopeCompany{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeCompany<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeCompany))
				return null;

			return OnGeneratePredicateWithPermissionScopeCompany<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		/// <summary>
		/// Returns <paramref name="leftHandQueryable"/> or calls <see cref="GetEmptyQueryPredicate"/> if <paramref name="leftHandQueryable"/> is null.
		/// Override to build the relevant permission scope <see cref="IQueryable"/>.
		/// </summary>
		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeCompany<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? GetEmptyQueryPredicate());
		}

		/// <summary>
		/// Builds the relevant <see cref="IQueryable"/> for <see cref="QueryStrategy.WithPermissionScopeCompanyAndUser{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IQueryable<TData> GeneratePredicateWithPermissionScopeCompanyAndUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			var queryStrategy = GetNullQueryStrategy() as QueryStrategy;

			if (queryStrategy == null)
				return null;

			if (queryPredicate.Name != GetFunctionNameOfType<TAuthenticationToken>(queryStrategy.WithPermissionScopeCompanyAndUser))
				return null;

			return OnGeneratePredicateWithPermissionScopeCompanyAndUser<TAuthenticationToken>(queryPredicate, leftHandQueryable);
		}

		/// <summary>
		/// Returns <paramref name="leftHandQueryable"/> or calls <see cref="GetEmptyQueryPredicate"/> if <paramref name="leftHandQueryable"/> is null.
		/// Override to build the relevant permission scope <see cref="IQueryable"/>.
		/// </summary>
		protected virtual IQueryable<TData> OnGeneratePredicateWithPermissionScopeCompanyAndUser<TAuthenticationToken>(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable)
		{
			return (leftHandQueryable ?? GetEmptyQueryPredicate());
		}

		/// <summary>
		/// Override to build an <see cref="IQueryable"/> from the <paramref name="queryPredicate"/> and an optional <paramref name="leftHandQueryable"/>.
		/// </summary>
		protected abstract IQueryable<TData> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null);

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionNameOfType<TParameter1>(Func<TParameter1, IQueryPredicate> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionName<T>(Func<T> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionName<TParameter1>(Func<TParameter1, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionName<TParameter1, TParameter2>(Func<TParameter1, TParameter2, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionName<TParameter1, TParameter2, TParameter3>(Func<TParameter1, TParameter2, TParameter3, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionName<TParameter1, TParameter2, TParameter3, TParameter4>(Func<TParameter1, TParameter2, TParameter3, TParameter4, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Gets the Name of the method in <paramref name="expression"/>
		/// </summary>
		protected virtual string GetFunctionName<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5>(Func<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TQueryStrategy> expression)
		{
			return expression.Method.Name;
		}

		/// <summary>
		/// Uses <see cref="Activator.CreateInstance{T}"/> to create a new instance of <typeparamref name="TQueryStrategy"/>.
		/// </summary>
		protected virtual TQueryStrategy GetNullQueryStrategy()
		{
			try
			{
				return Activator.CreateInstance<TQueryStrategy>();
			}
			catch (MissingMethodException)
			{
				return DependencyResolver.Resolve<TQueryStrategy>();
			}
		}

		/// <summary>
		/// Override to build or apply any sorting required to <paramref name="queryable"/>
		/// </summary>
		/// <param name="queryStrategy">The <typeparamref name="TQueryStrategy"/> with sorting information.</param>
		/// <param name="queryable">The <see cref="IQueryable"/> to apply sorting to.</param>
		protected virtual void ApplySorting(TQueryStrategy queryStrategy, ref IQueryable<TData> queryable)
		{
		}
	}
}