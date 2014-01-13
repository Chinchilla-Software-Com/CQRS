using System.Linq;

namespace Cqrs.Repositories.Queries
{
	public interface IQueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
	{
		IQueryable<TData> CreateQueryable(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery);

		IQueryable<TData> CreateQueryable(ICollectionResultQuery<TQueryStrategy, TData> collectionResultQuery);
	}
}