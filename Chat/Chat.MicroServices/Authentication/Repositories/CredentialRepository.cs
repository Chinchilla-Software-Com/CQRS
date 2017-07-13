namespace Chat.MicroServices.Authentication.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Factories;
	using Queries.Strategies;

	public class CredentialRepository : Repository<CredentialQueryStrategy, CredentialQueryStrategyBuilder, CredentialEntity>, ICredentialRepository
	{
		public CredentialRepository(IDomainDataStoreFactory dataStoreFactory, CredentialQueryStrategyBuilder credentialQueryBuilder)
			: base(dataStoreFactory.GetCredentialDataStore, credentialQueryBuilder)
		{
		}
	}
}