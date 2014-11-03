using Cqrs.Configuration;

namespace Cqrs.Repositories.Queries
{
	public class QueryFactory : IQueryFactory
	{
		protected IDependencyResolver DependencyResolver { get; private set; }

		public QueryFactory(IDependencyResolver dependencyResolver)
		{
			DependencyResolver = dependencyResolver;
		}

		public ISingleResultQuery<TUserQueryStrategy, TData> CreateNewSingleResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy
		{
			var queryStrategy = DependencyResolver.Resolve<TUserQueryStrategy>();
			return new SingleResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}

		public ICollectionResultQuery<TUserQueryStrategy, TData> CreateNewCollectionResultQuery<TUserQueryStrategy, TData>()
		where TUserQueryStrategy : IQueryStrategy
		{
			var queryStrategy = DependencyResolver.Resolve<TUserQueryStrategy>();
			return new CollectionResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}
	}
}