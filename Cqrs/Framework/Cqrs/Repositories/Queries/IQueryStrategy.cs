namespace Cqrs.Repositories.Queries
{
	public interface IQueryStrategy
	{
		IQueryPredicate QueryPredicate { get; }
	}
}