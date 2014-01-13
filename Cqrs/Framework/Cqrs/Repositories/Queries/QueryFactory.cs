namespace Cqrs.Repositories.Queries
{
	public class QueryFactory : IQueryFactory
	{
		public ISingleResultQuery<TUserQueryStrategy, TData> CreateNewSingleResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy, new()
		{
			var queryStrategy = new TUserQueryStrategy();
			return new SingleResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}

		public ICollectionResultQuery<TUserQueryStrategy, TData> CreateNewCollectionResultQuery<TUserQueryStrategy, TData>()
		where TUserQueryStrategy : IQueryStrategy, new()
		{
			var queryStrategy = new TUserQueryStrategy();
			return new CollectionResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}
	}
}