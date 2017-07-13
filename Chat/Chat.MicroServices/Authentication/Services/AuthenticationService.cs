namespace Chat.MicroServices.Authentication.Services
{
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Entities;
	using Repositories;
	using Repositories.Queries.Strategies;
	using System;
	using System.Security.Cryptography;
	using System.Text;

	public class AuthenticationService : IAuthenticationService
	{
		public AuthenticationService(ICredentialRepository credentialRepository, IQueryFactory queryFactory)
		{
			CredentialRepository = credentialRepository;
			QueryFactory = queryFactory;
		}

		protected ICredentialRepository CredentialRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		/// <summary>
		/// Validate the provided <paramref name="emailAddress"/> and <paramref name="password"/> match a valid set of credentials
		/// </summary>
		/// <returns>The <see cref="UserEntity.Rsn"/></returns>
		public virtual IServiceResponseWithResultData<Guid?> ValidateCredentials(string emailAddress, string password)
		{
			// Define Query
			ISingleResultQuery<CredentialQueryStrategy, CredentialEntity> query = QueryFactory.CreateNewSingleResultQuery<CredentialQueryStrategy, CredentialEntity>();

			string hash = GenerateCredentialHash(emailAddress, password);
			query.QueryStrategy.WithHash(hash);

			// Retrieve Data
			query = CredentialRepository.Retrieve(query, false);
			CredentialEntity queryResults = query.Result;

			return new ServiceResponseWithResultData<Guid?>
			{
				State = queryResults == null ? ServiceResponseStateType.FailedAuthentication : ServiceResponseStateType.Succeeded,
				ResultData = queryResults == null ? (Guid?)null : queryResults.UserRsn
			};
		}

		private const string Salt1 = "a6f723b251304867bdada865f4d93694";
		private const string Salt2 = "b71a8bc00f1d41fb804921febac180d2";

		internal virtual string GenerateCredentialHash(string emailAddress, string password)
		{
			SHA512 sha512 = SHA512.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(string.Format("{0}::{1}::{2}::{3}", Salt1, emailAddress, Salt2, password));
			byte[] hash = sha512.ComputeHash(bytes);
			return Encoding.UTF8.GetString(hash);
		}
	}
}