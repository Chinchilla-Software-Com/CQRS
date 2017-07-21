namespace $safeprojectname$.Authentication.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Queries.Strategies;

	public interface ICredentialRepository : IRepository<CredentialQueryStrategy, CredentialEntity>
	{
	}
}