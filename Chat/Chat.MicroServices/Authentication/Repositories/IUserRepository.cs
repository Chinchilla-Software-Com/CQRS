namespace Chat.MicroServices.Authentication.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Queries.Strategies;

	public interface IUserRepository : IRepository<UserQueryStrategy, UserEntity>
	{
	}
}