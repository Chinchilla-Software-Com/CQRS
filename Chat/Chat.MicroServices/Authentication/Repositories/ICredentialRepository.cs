using Chat.MicroServices.Authentication.Entities;

namespace Chat.MicroServices.Authentication.Repositories
{
	using Cqrs.Repositories;
	using Queries.Strategies;

	public interface ICredentialRepository : IRepository<CredentialQueryStrategy, CredentialEntity>
	{
	}
}