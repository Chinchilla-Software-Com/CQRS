namespace Cqrs.Repositories.Queries
{
	public interface IAndQueryPredicate : IQueryPredicate
	{
		IQueryPredicate LeftQueryPredicate { get; }

		IQueryPredicate RightQueryPredicate { get; }
	}
}