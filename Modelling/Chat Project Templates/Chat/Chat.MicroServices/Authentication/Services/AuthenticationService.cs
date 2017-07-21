namespace $safeprojectname$.Authentication.Services
{
	using System;
	using System.Runtime.Serialization;
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Commands;
	using Cqrs.Events;
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Entities;
	using Helpers;
	using Repositories;
	using Repositories.Queries.Strategies;

	/// <summary>
	/// A WCF service for accessing and modifying credentials.
	/// </summary>
	[DataContract(Namespace = "https://getcqrs.net/Authentication/1001/")]
	public class AuthenticationService : EventService<Guid>, IAuthenticationService
	{
		/// <summary>
		/// Instantiate a new instance of the <see cref="AuthenticationService"/> class.
		/// </summary>
		public AuthenticationService(IEventStore<Guid> eventStore, ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IQueryFactory queryFactory, ICredentialRepository credentialRepository, IAuthenticationHashHelper authenticationHashHelper, ICommandPublisher<Guid> commandPublisher)
			: base(eventStore, logger, correlationIdHelper, authenticationTokenHelper)
		{
			CredentialRepository = credentialRepository;
			QueryFactory = queryFactory;
			AuthenticationHashHelper = authenticationHashHelper;
			CommandPublisher = commandPublisher;
		}

		protected ICredentialRepository CredentialRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IAuthenticationHashHelper AuthenticationHashHelper { get; private set; }

		protected ICommandPublisher<Guid> CommandPublisher { get; private set; }

		/// <summary>
		/// Validate the provided <paramref name="serviceRequest">credentials</paramref> are valid.
		/// </summary>
		/// <param name="serviceRequest">The user credentials to validate.</param>
		/// <returns>The users identifier.</returns>
		public virtual IServiceResponseWithResultData<Guid?> Login(IServiceRequestWithData<Guid, LoginParameters> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			var userLogin = serviceRequest.Data;

			if (userLogin == null || string.IsNullOrEmpty(userLogin.EmailAddress) || string.IsNullOrEmpty(userLogin.Password))
				return CompleteResponse(new ServiceResponseWithResultData<Guid?> { State = ServiceResponseStateType.FailedValidation });

			// Define Query
			ISingleResultQuery<CredentialQueryStrategy, CredentialEntity> query = QueryFactory.CreateNewSingleResultQuery<CredentialQueryStrategy, CredentialEntity>();

			string hash = AuthenticationHashHelper.GenerateCredentialHash(userLogin.EmailAddress, userLogin.Password);
			query.QueryStrategy.WithHash(hash);

			// Retrieve Data
			query = CredentialRepository.Retrieve(query, false);
			CredentialEntity queryResults = query.Result;

			var responseData = new ServiceResponseWithResultData<Guid?>
			{
				State = queryResults == null ? ServiceResponseStateType.FailedAuthentication : ServiceResponseStateType.Succeeded,
				ResultData = queryResults == null ? (Guid?)null : queryResults.UserRsn
			};

			// Complete the response
			ServiceResponseWithResultData<Guid?> response = CompleteResponse(responseData);

			return response;
		}

		/// <summary>
		/// WCF parameters to validate credentials.
		/// </summary>
		[Serializable]
		[DataContract(Namespace = "https://getcqrs.net/Authentication/1001/")]
		public class LoginParameters
		{
			/// <summary>
			/// The user's email address
			/// </summary>
			[DataMember]
			public string EmailAddress { get; set; }

			/// <summary>
			/// The user's password
			/// </summary>
			[DataMember]
			public string Password { get; set; }
		}
	}
}