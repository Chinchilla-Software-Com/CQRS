namespace $safeprojectname$.Authentication.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Factories;
	using Queries.Strategies;

	public class UserRepository : Repository<UserQueryStrategy, UserQueryStrategyBuilder, UserEntity>, IUserRepository
	{
		public UserRepository(IDomainDataStoreFactory dataStoreFactory, UserQueryStrategyBuilder userQueryBuilder)
			: base(dataStoreFactory.GetUserDataStore, userQueryBuilder)
		{
		}
	}
}