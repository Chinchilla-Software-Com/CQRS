namespace Cqrs.Repositories.Queries
{
	public interface IQueryWithStrategy<TQueryStrategy>
		where TQueryStrategy : IQueryStrategy 
	{
		TQueryStrategy QueryStrategy { get; set; }
	}
}