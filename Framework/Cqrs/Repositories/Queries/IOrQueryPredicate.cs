namespace Cqrs.Repositories.Queries
{
	public interface IOrQueryPredicate : IQueryPredicate
	{
		IQueryPredicate LeftQueryPredicate { get; }

		IQueryPredicate RightQueryPredicate { get; }
	}
}