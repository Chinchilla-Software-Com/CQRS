namespace Chat.Api.Controllers
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
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

	[RoutePrefix("Authentication")]
	public class AuthenticationController : CqrsApiController<Guid>
	{
		public AuthenticationController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IQueryFactory queryFactory, ICredentialRepository credentialRepository, IAuthenticationHashHelper authenticationHashHelper)
			: base(logger, correlationIdHelper, authenticationTokenHelper)
		{
			QueryFactory = queryFactory;
			CredentialRepository = credentialRepository;
			AuthenticationHashHelper = authenticationHashHelper;
		}

		protected ICredentialRepository CredentialRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IAuthenticationHashHelper AuthenticationHashHelper { get; private set; }

		[Route("Login")]
		[HttpPost]
		public HttpResponseMessage Login(UserLogin userLogin)
		{
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
			HttpResponseMessage response = CompleteResponse(responseData);

			// If authentication has succeeded then return now.
			if (responseData.State != ServiceResponseStateType.Succeeded || responseData.ResultData == null)
				return response;

			// Copy encrypted auth token to X-Token for SignalR
			var cookie = new CookieHeaderValue("X-Token", responseData.ResultData.Value.ToString("N"))
			{
				Expires = DateTimeOffset.Now.AddDays(1),
			};
			response.Headers.AddCookies(new[] { cookie });

			return response;
		}

		[Route("Logout")]
		[HttpDelete]
		public HttpResponseMessage Logout()
		{
			var responseData = new ServiceResponse
			{
				State = ServiceResponseStateType.Succeeded
			};

			// Complete the response
			HttpResponseMessage response = CompleteResponse(responseData);

			// Clear encrypted auth token from the UI
			var cookie = new CookieHeaderValue("X-Token", string.Empty)
			{
				Expires = DateTimeOffset.Now,
			};
			response.Headers.AddCookies(new[] { cookie });

			return response;
		}
	}
}