namespace Cqrs.Repositories.Queries
{
	public interface IQueryFactory
	{
		ISingleResultQuery<TUserQueryStrategy, TData> CreateNewSingleResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy, new();

		ICollectionResultQuery<TUserQueryStrategy, TData> CreateNewCollectionResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy, new();
	}
}