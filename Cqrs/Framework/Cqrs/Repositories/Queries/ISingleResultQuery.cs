namespace Cqrs.Repositories.Queries
{
	public interface ISingleResultQuery<TQueryStrategy, out TData> : IQueryWithStrategy<TQueryStrategy>, IQueryWithResults<TData>
		where TQueryStrategy : IQueryStrategy 
	{
	}
}