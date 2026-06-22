#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.Azure.DocumentDb.DataStores;
using Cqrs.Azure.DocumentDb.Entities;
using Cqrs.DataStores;
using Cqrs.Repositories;
using Cqrs.Repositories.Queries;

namespace Cqrs.Azure.DocumentDb.Repositories
{
	/// <summary>
	/// Provides basic repository methods for operations with an <see cref="IDataStore{TData}"/> using Azure DocumentDB (CosmosDB).
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TQueryBuilder">The <see cref="Type"/> of the <see cref="QueryBuilder{TQueryStrategy, TData}"/> that will be used to build queries.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data held in storage.</typeparam>
	public abstract class AzureRepository<TQueryStrategy, TQueryBuilder, TData> : Repository<TQueryStrategy, TQueryBuilder, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : AzureDocumentDbEntity
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureRepository{TQueryStrategy,TQueryBuilder,TData}"/>
		/// </summary>
		protected AzureRepository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
			:base(createDataStoreFunction, queryBuilder)
		{
		}

		/// <summary>
		/// Load the <typeparamref name="TData"/> from Azure DocumentDB (CosmosDB) identified by the provided <paramref name="rsn"/>.
		/// </summary>
		/// <param name="rsn">The identifier if the <typeparamref name="TData"/> to load.</param>
		/// <param name="throwExceptionOnMissingEntity">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>
		public override TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			using (var dataStore = CreateDataStoreFunction() as AzureDocumentDbDataStore<TData>)
			{
				if (throwExceptionOnMissingEntity)
					return dataStore.Single(entity => entity.Rsn == rsn);
				return dataStore.SingleOrDefault(entity => entity.Rsn == rsn);
			}
		}

		/// <summary>
		/// Calls <see cref="Repository{TQueryStrategy,TQueryBuilder,TData}.CreateDataStoreFunction"/> passing the <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A function defining a filter if required.</param>
		protected override IQueryable<TData> CreateQueryable(Expression<Func<TData, bool>> predicate)
		{
			return CreateDataStoreFunction().AsQueryable().Where(predicate);
		}

	}
}