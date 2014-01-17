using Cqrs.Config;

namespace Cqrs.Repositories.Queries
{
	public class QueryFactory : IQueryFactory
	{
		protected IServiceLocator DependencyResolver { get; private set; }

		public QueryFactory(IServiceLocator dependencyResolver)
		{
			DependencyResolver = dependencyResolver;
		}

		public ISingleResultQuery<TUserQueryStrategy, TData> CreateNewSingleResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy
		{
			var queryStrategy = DependencyResolver.GetService<TUserQueryStrategy>();
			return new SingleResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}

		public ICollectionResultQuery<TUserQueryStrategy, TData> CreateNewCollectionResultQuery<TUserQueryStrategy, TData>()
		where TUserQueryStrategy : IQueryStrategy
		{
			var queryStrategy = DependencyResolver.GetService<TUserQueryStrategy>();
			return new CollectionResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}
	}
}