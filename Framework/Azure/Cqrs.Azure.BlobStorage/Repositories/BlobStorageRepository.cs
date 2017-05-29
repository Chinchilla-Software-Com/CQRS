using System;
using Cqrs.DataStores;
using Cqrs.Repositories;
using Cqrs.Entities;
using Cqrs.Repositories.Queries;

namespace Cqrs.Azure.BlobStorage.Repositories
{
	public class BlobStorageRepository<TQueryStrategy, TQueryBuilder, TData> : Repository<TQueryStrategy, TQueryBuilder, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : Entity, new()
	{
		public BlobStorageRepository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
			: base(createDataStoreFunction, queryBuilder)
		{
		}

		#region Overrides of Repository<TQueryStrategy,TQueryBuilder,TData>

		public override TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			using (IDataStore<TData> dataStore = CreateDataStoreFunction())
			{
				try
				{
					return dataStore.GetByName(rsn);
				}
				catch (InvalidOperationException exception)
				{
					if (throwExceptionOnMissingEntity && exception.Message == "Sequence contains no elements")
						throw;
				}

				return null;
			}
		}

		#endregion
	}
}