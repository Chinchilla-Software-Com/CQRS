namespace $safeprojectname$.Helpers
{
	using Cqrs.Authentication;
	using Cqrs.Repositories.Queries;
	using MicroServices.Authentication.Entities;
	using MicroServices.Authentication.Repositories;
	using MicroServices.Authentication.Repositories.Queries.Strategies;
	using System;

	public class AuthenticationHelper : IAuthenticationHelper
	{
		public AuthenticationHelper(IQueryFactory queryFactory, IUserRepository userRepository, IAuthenticationTokenHelper<Guid> authenticationTokenHelper)
		{
			QueryFactory = queryFactory;
			UserRepository = userRepository;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		protected IUserRepository UserRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IAuthenticationTokenHelper<Guid> AuthenticationTokenHelper { get; private set; }

		public string GetCurrentUsersName()
		{
			// Define Query
			ISingleResultQuery<UserQueryStrategy, UserEntity> query = QueryFactory.CreateNewSingleResultQuery<UserQueryStrategy, UserEntity>();

			query.QueryStrategy.WithRsn(GetCurrentUser());

			// Retrieve Data
			query = UserRepository.Retrieve(query);

			return query.Result.FirstName;
		}

		public Guid GetCurrentUser()
		{
			return AuthenticationTokenHelper.GetAuthenticationToken();
		}
	}
}