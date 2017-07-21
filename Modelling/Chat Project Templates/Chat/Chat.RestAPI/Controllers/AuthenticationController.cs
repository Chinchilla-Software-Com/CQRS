namespace $safeprojectname$.Controllers
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Cqrs.WebApi;
	using MicroServices.Authentication.Entities;
	using MicroServices.Authentication.Helpers;
	using MicroServices.Authentication.Repositories;
	using MicroServices.Authentication.Repositories.Queries.Strategies;
	using Models;
	using System;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web.Http;

	/// <summary>
	/// A WebAPI RESTful service for validating user credential.
	/// </summary>
	[RoutePrefix("Authentication")]
	public class AuthenticationController : CqrsApiController<Guid>
	{
		public AuthenticationController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IConfigurationManager configurationManager, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IQueryFactory queryFactory, ICredentialRepository credentialRepository, IAuthenticationHashHelper authenticationHashHelper)
			: base(logger, correlationIdHelper, configurationManager, authenticationTokenHelper)
		{
			QueryFactory = queryFactory;
			CredentialRepository = credentialRepository;
			AuthenticationHashHelper = authenticationHashHelper;
		}

		protected ICredentialRepository CredentialRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IAuthenticationHashHelper AuthenticationHashHelper { get; private set; }

		/// <summary>
		/// Validate the provided <paramref name="userLogin">credentials</paramref> are valid.
		/// This also sets the users id into a <see cref="CookieHeaderValue"/> named after the value of the application settings "Cqrs.Web.AuthenticationTokenName"
		/// </summary>
		/// <param name="userLogin">The user credentials to validate.</param>
		/// <returns>The users id.</returns>
		[Route("Login")]
		[HttpPost]
		public HttpResponseMessage<ServiceResponseWithResultData<Guid?>> Login(UserLogin userLogin)
		{
			if (userLogin == null || string.IsNullOrEmpty(userLogin.EmailAddress) || string.IsNullOrEmpty(userLogin.Password))
				return CompleteResponseWithData(new ServiceResponseWithResultData<Guid?> { State = ServiceResponseStateType.FailedValidation });
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
			HttpResponseMessage<ServiceResponseWithResultData<Guid?>> response = CompleteResponseWithData(responseData);

			// If authentication has succeeded then return now.
			if (responseData.State != ServiceResponseStateType.Succeeded || responseData.ResultData == null)
				return response;

			// Copy encrypted auth token to X-Token for SignalR
			string authenticationTokenName = DependencyResolver.Current.Resolve<IConfigurationManager>().GetSetting("Cqrs.Web.AuthenticationTokenName") ?? "X-Token";
			var cookie = new CookieHeaderValue(authenticationTokenName, responseData.ResultData.Value.ToString("N"))
			{
				Expires = DateTimeOffset.Now.AddDays(1),
			};
			response.Headers.AddCookies(new[] { cookie });

			return response;
		}

		/// <summary>
		/// Clears the value of the <see cref="CookieHeaderValue"/> named after the value of the application settings "Cqrs.Web.AuthenticationTokenName".
		/// </summary>
		[Route("Logout")]
		[HttpDelete]
		public HttpResponseMessage<ServiceResponse> Logout()
		{
			var responseData = new ServiceResponse
			{
				State = ServiceResponseStateType.Succeeded
			};

			// Complete the response
			HttpResponseMessage<ServiceResponse> response = CompleteResponseWithData(responseData);

			// Clear encrypted auth token from the UI
			string authenticationTokenName = DependencyResolver.Current.Resolve<IConfigurationManager>().GetSetting("Cqrs.Web.AuthenticationTokenName") ?? "X-Token";
			var cookie = new CookieHeaderValue(authenticationTokenName, string.Empty)
			{
				Expires = DateTimeOffset.Now,
			};
			response.Headers.AddCookies(new[] { cookie });

			return response;
		}
	}
}