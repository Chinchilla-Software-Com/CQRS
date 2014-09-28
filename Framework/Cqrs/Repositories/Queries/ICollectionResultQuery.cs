using System.Collections.Generic;

namespace Cqrs.Repositories.Queries
{
	public interface ICollectionResultQuery<TQueryStrategy, out TData> : IQueryWithStrategy<TQueryStrategy>, IQueryWithResults<IEnumerable<TData>>
		where TQueryStrategy : IQueryStrategy 
	{
	}
}