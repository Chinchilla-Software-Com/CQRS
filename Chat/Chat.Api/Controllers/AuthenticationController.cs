using System.Net;
using System.Web.Http.ModelBinding;

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
	using System.Net.Http.Formatting;
	using System.Net.Http.Headers;
	using System.Web.Http;
	using System.Web.ModelBinding;

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
				return CompleteResponse<ServiceResponseWithResultData<Guid?>, Guid?>(new ServiceResponseWithResultData<Guid?> { State = ServiceResponseStateType.FailedValidation });
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
			HttpResponseMessage response = CompleteResponse<ServiceResponseWithResultData<Guid?>, Guid?>(responseData);

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

		[HttpPost]
		public HttpResponseMessage Logout()
		{
			var responseData = new ServiceResponseWithResultData<dynamic>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = new { xToken = string.Empty }
			};

			// Clear encrypted auth token from the UI
			var cookie = new CookieHeaderValue("X-Token", responseData.ResultData.xToken)
			{
				Expires = DateTimeOffset.Now,
			};
			var response = new HttpResponseMessage();
			response.Headers.AddCookies(new[] { cookie });

			CompleteResponse(responseData);

			return response;
		}

		protected virtual HttpResponseMessage CompleteResponse<TServiceResponse, TData>(TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponseWithResultData<TData>
		{
			serviceResponse.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			var response = new HttpResponseMessage();

			HttpConfiguration configuration = Request.GetConfiguration();
			var contentNegotiator = configuration.Services.GetContentNegotiator();
			ContentNegotiationResult negotiationResult = contentNegotiator.Negotiate(typeof(IServiceResponseWithResultData<TData>), Request, configuration.Formatters);

			response.Content = new ObjectContent<IServiceResponseWithResultData<TData>>(serviceResponse, negotiationResult.Formatter, negotiationResult.MediaType);

			switch (serviceResponse.State)
			{
				case ServiceResponseStateType.Succeeded:
					response.StatusCode = HttpStatusCode.Accepted;
					break;
				case ServiceResponseStateType.FailedAuthentication:
					response.StatusCode = HttpStatusCode.Forbidden;
					break;
				case ServiceResponseStateType.FailedAuthorisation:
					response.StatusCode = HttpStatusCode.Unauthorized;
					break;
				case ServiceResponseStateType.FailedValidation:
					response.StatusCode = HttpStatusCode.PreconditionFailed;
					break;
				case ServiceResponseStateType.FailedWithAFatalException:
					response.StatusCode = HttpStatusCode.InternalServerError;
					break;
				case ServiceResponseStateType.FailedWithAnUnexpectedException:
					response.StatusCode = HttpStatusCode.InternalServerError;
					break;
				case ServiceResponseStateType.Unknown:
					response.StatusCode = HttpStatusCode.BadRequest;
					break;
				default:
					response.StatusCode = HttpStatusCode.Ambiguous;
					break;
			}

			return response;
		}
	}
}