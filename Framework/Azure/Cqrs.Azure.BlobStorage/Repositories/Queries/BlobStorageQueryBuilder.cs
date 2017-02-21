using System.Linq;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;
using Cqrs.Repositories.Queries;

namespace Cqrs.Azure.BlobStorage.Repositories.Queries
{
	public abstract class BlobStorageQueryBuilder<TQueryStrategy, TData>
		: QueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TData : Entity
	{
		protected BlobStorageQueryBuilder(IDataStore<TData> dataStore, IDependencyResolver dependencyResolver)
			: base(dataStore, dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<TQueryStrategy,TData>

		protected override IQueryable<TData> GetEmptyQueryPredicate()
		{
			return DataStore.GetByFolder().AsQueryable();
		}

		#endregion
	}
}