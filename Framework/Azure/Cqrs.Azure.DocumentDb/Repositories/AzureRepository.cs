#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
	public abstract class AzureRepository<TQueryStrategy, TQueryBuilder, TData> :  Repository<TQueryStrategy, TQueryBuilder, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : AzureDocumentDbEntity
	{
		protected AzureRepository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
			:base(createDataStoreFunction, queryBuilder)
		{
		}

		public override TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			using (var dataStore = CreateDataStoreFunction() as AzureDocumentDbDataStore<TData>)
			{
				if (throwExceptionOnMissingEntity)
					return dataStore.Single(entity => entity.Rsn == rsn);
				return dataStore.SingleOrDefault(entity => entity.Rsn == rsn);
			}
		}

		protected override IQueryable<TData> CreateQueryable(Expression<Func<TData, bool>> predicate)
		{
			return CreateDataStoreFunction().AsQueryable().Where(predicate);
		}

	}
}