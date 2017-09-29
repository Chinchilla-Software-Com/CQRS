#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Services;
using System.Net.Http.Formatting;
using System.Web;
using Cqrs.Configuration;

namespace Cqrs.WebApi
{
	/// <summary>
	/// A <see cref="ApiController"/> that expects the <see cref="ISingleSignOnToken.Token"/> to be sent as a <see cref="HttpHeaders"/> with a key whose name is defined by the <see cref="System.Configuration.ConfigurationManager.AppSettings"/> "Cqrs.Web.AuthenticationTokenName", in accordance with OAuth specifications
	/// </summary>
	/// <remarks>
	/// See https://www.asp.net/web-api/overview/getting-started-with-aspnet-web-api/creating-api-help-pages for details on adding WebApi Help Pages.
	/// </remarks>
	public abstract class CqrsApiController
		: ApiController
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="CqrsApiController"/>.
		/// </summary>
		protected CqrsApiController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IConfigurationManager configurationManager)
		{
			CorrelationIdHelper = correlationIdHelper;
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		/// <summary>
		/// Gets or set the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Extracts the authentication token looking for a <see cref="KeyValuePair{TKey,TValue}"/> where the key as defined by the <see cref="System.Configuration.ConfigurationManager.AppSettings"/> "Cqrs.Web.AuthenticationTokenName",
		/// from the <see cref="HttpRequest.Headers"/>, if one isn't found we then try the <see cref="HttpRequest.Cookies"/>
		/// </summary>
		protected virtual string GetToken()
		{
			string authenticationTokenName = ConfigurationManager.GetSetting("Cqrs.Web.AuthenticationTokenName") ?? "X-Token";

			string xToken = null;
			IEnumerable<string> tokenValue;
			if (Request.Headers.TryGetValues(authenticationTokenName, out tokenValue))
				xToken = tokenValue.First();
			else
			{
				CookieHeaderValue cookie = Request.Headers.GetCookies(authenticationTokenName).FirstOrDefault();
				if (cookie != null)
					xToken = cookie[authenticationTokenName].Value;
			}

			return xToken;
		}

		/// <summary>
		/// Create a <see cref="IServiceRequest{TAuthenticationToken}"/> setting header information.
		/// </summary>
		protected virtual IServiceRequest<TSingleSignOnToken> CreateRequest<TSingleSignOnToken>()
			where TSingleSignOnToken : ISingleSignOnToken, new()
		{
			return new ServiceRequest<TSingleSignOnToken>
			{
				AuthenticationToken = CreateAuthenticationToken<TSingleSignOnToken>(),
				CorrelationId = CorrelationIdHelper.GetCorrelationId()
			};
		}

		/// <summary>
		/// Create a <see cref="IServiceRequestWithData{TAuthenticationToken,TData}"/> setting header information.
		/// If <paramref name="createParameterDelegate"/> is not null, it is used to populate <see cref="IServiceRequestWithData{TAuthenticationToken,TData}.Data"/> otherwise <see cref="CreateParameter{TParameters}"/> is used.
		/// </summary>
		protected virtual IServiceRequestWithData<TSingleSignOnToken, TParameters> CreateRequestWithData<TSingleSignOnToken, TParameters>(Func<TParameters> createParameterDelegate = null)
			where TParameters : new()
		{
			return new ServiceRequestWithData<TSingleSignOnToken, TParameters>
			{
				AuthenticationToken = CreateAuthenticationToken<TSingleSignOnToken>(),
				CorrelationId = CorrelationIdHelper.GetCorrelationId(),
				Data = createParameterDelegate == null ? CreateParameter<TParameters>() : createParameterDelegate()
			};
		}

		/// <summary>
		/// Create an <typeparamref name="TSingleSignOnToken"/>.
		/// </summary>
		/// <typeparam name="TSingleSignOnToken">The <see cref="Type"/> of <see cref="ISingleSignOnToken"/>.</typeparam>
		protected virtual TSingleSignOnToken CreateAuthenticationToken<TSingleSignOnToken>()
		{
			if (typeof(TSingleSignOnToken) == typeof(int))
				return (TSingleSignOnToken)(object)int.Parse(GetToken());
			if (typeof(TSingleSignOnToken) == typeof(Guid))
				return (TSingleSignOnToken)(object)new Guid(GetToken());
			if (typeof(TSingleSignOnToken) == typeof(string))
				return (TSingleSignOnToken)(object)GetToken();

			if (typeof(TSingleSignOnToken) == typeof(SingleSignOnToken))
				return (TSingleSignOnToken)(object)new SingleSignOnToken
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			if (typeof(TSingleSignOnToken) == typeof(SingleSignOnTokenWithUserRsn))
				return (TSingleSignOnToken)(object)new SingleSignOnTokenWithUserRsn
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			if (typeof(TSingleSignOnToken) == typeof(SingleSignOnTokenWithCompanyRsn))
				return (TSingleSignOnToken)(object)new SingleSignOnTokenWithCompanyRsn
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			if (typeof(TSingleSignOnToken) == typeof(SingleSignOnTokenWithUserRsnAndCompanyRsn))
				return (TSingleSignOnToken)(object)new SingleSignOnTokenWithUserRsnAndCompanyRsn
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};

			if (typeof(TSingleSignOnToken) == typeof(ISingleSignOnToken))
				return (TSingleSignOnToken)(object)new SingleSignOnToken
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			if (typeof(TSingleSignOnToken) == typeof(ISingleSignOnTokenWithUserRsn))
				return (TSingleSignOnToken)(object)new SingleSignOnTokenWithUserRsn
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			if (typeof(TSingleSignOnToken) == typeof(ISingleSignOnTokenWithCompanyRsn))
				return (TSingleSignOnToken)(object)new SingleSignOnTokenWithCompanyRsn
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			if (typeof(TSingleSignOnToken) == typeof(ISingleSignOnTokenWithUserRsnAndCompanyRsn))
				return (TSingleSignOnToken)(object)new SingleSignOnTokenWithUserRsnAndCompanyRsn
				{
					DateIssued = GetDateTokenIssued(),
					Token = GetToken(),
					TimeOfExpiry = GetTokenTimeOfExpiry()
				};
			return default(TSingleSignOnToken);
		}

		/// <summary>
		/// Creates a blank <typeparamref name="TParameters"/>
		/// </summary>
		protected virtual TParameters CreateParameter<TParameters>()
			where TParameters : new()
		{
			return new TParameters();
		}

		/// <summary>
		/// Get the <see cref="DateTime"/> the current authentication token was issued.
		/// </summary>
		/// <returns>default(DateTime)</returns>
		protected virtual DateTime GetDateTokenIssued()
		{
			return default(DateTime);
		}

		/// <summary>
		/// Get the <see cref="DateTime"/> the current authentication token will expire.
		/// </summary>
		/// <returns>default(DateTime)</returns>
		protected virtual DateTime GetTokenTimeOfExpiry()
		{
			return default(DateTime);
		}

		/// <summary>
		/// Completes the provided <paramref name="response"/> by setting the appropriate <see cref="HttpResponseMessage.StatusCode"/> and populating <see cref="HttpResponseMessage.Content"/> with <paramref name="serviceResponse"/>.
		/// </summary>
		protected virtual HttpResponseMessage CompleteResponse<TServiceResponse>(HttpResponseMessage response, TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponse
		{
			serviceResponse.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			HttpConfiguration configuration = Request.GetConfiguration();
			var contentNegotiator = configuration.Services.GetContentNegotiator();
			ContentNegotiationResult negotiationResult = contentNegotiator.Negotiate(typeof(IServiceResponse), Request, configuration.Formatters);

			response.Content = new ObjectContent<IServiceResponse>(serviceResponse, negotiationResult.Formatter, negotiationResult.MediaType);

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

		/// <summary>
		/// Creates a new <see cref="HttpResponseMessage"/> and completes the response by setting the appropriate <see cref="HttpResponseMessage.StatusCode"/> and populating <see cref="HttpResponseMessage.Content"/> with <paramref name="serviceResponse"/>.
		/// </summary>
		protected virtual HttpResponseMessage CompleteResponse<TServiceResponse>(TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponse
		{
			var response = new HttpResponseMessage();

			return CompleteResponse(response, serviceResponse);
		}

		/// <summary>
		/// Creates a new <see cref="HttpResponseMessage"/> and completes the response by setting the appropriate <see cref="HttpResponseMessage.StatusCode"/> and populating <see cref="HttpResponseMessage.Content"/> with <paramref name="serviceResponse"/>.
		/// </summary>
		protected virtual HttpResponseMessage<TServiceResponse> CompleteResponseWithData<TServiceResponse>(TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponse
		{
			var response = new HttpResponseMessage<TServiceResponse>();

			CompleteResponse(response, serviceResponse);

			return response;
		}
	}

	/// <summary>
	/// A <see cref="ApiController"/> that expects the <see cref="ISingleSignOnToken.Token"/> to be sent as a <see cref="HttpHeaders"/> with a key of "X-Token", in accordance with OAuth specifications
	/// </summary>
	/// <remarks>
	/// See https://www.asp.net/web-api/overview/getting-started-with-aspnet-web-api/creating-api-help-pages for details on adding WebApi Help Pages.
	/// </remarks>
	public abstract class CqrsApiController<TAuthenticationToken>
		: CqrsApiController
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="CqrsApiController"/>.
		/// </summary>
		protected CqrsApiController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IConfigurationManager configurationManager, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			: base(logger, correlationIdHelper, configurationManager)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		/// <summary>
		/// Gets or set the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Reads the current authentication token for the request from <see cref="AuthenticationTokenHelper"/>.
		/// </summary>
		protected override string GetToken()
		{
			TAuthenticationToken token = AuthenticationTokenHelper.GetAuthenticationToken();
			if (token != null)
				return token.ToString();
			return null;
		}

		/// <summary>
		/// Create a <see cref="IServiceRequest{TAuthenticationToken}"/> setting header information.
		/// </summary>
		protected virtual IServiceRequest<TAuthenticationToken> CreateRequest()
		{
			TAuthenticationToken token = AuthenticationTokenHelper.GetAuthenticationToken();
			return new ServiceRequest<TAuthenticationToken>
			{
				AuthenticationToken = token,
				CorrelationId = CorrelationIdHelper.GetCorrelationId()
			};
		}

		/// <summary>
		/// Create a <see cref="IServiceRequestWithData{TAuthenticationToken,TData}"/> setting header information.
		/// If <paramref name="createParameterDelegate"/> is not null, it is used to populate <see cref="IServiceRequestWithData{TAuthenticationToken,TData}.Data"/> otherwise <see cref="CqrsApiController.CreateParameter{TParameters}"/> is used.
		/// </summary>
		protected virtual IServiceRequestWithData<TAuthenticationToken, TParameters> CreateRequestWithData<TParameters>(Func<TParameters> createParameterDelegate = null)
			where TParameters : new()
		{
			TAuthenticationToken token = AuthenticationTokenHelper.GetAuthenticationToken();
			return new ServiceRequestWithData<TAuthenticationToken, TParameters>
			{
				AuthenticationToken = token,
				CorrelationId = CorrelationIdHelper.GetCorrelationId(),
				Data = createParameterDelegate == null ? CreateParameter<TParameters>() : createParameterDelegate()
			};
		}
	}
}