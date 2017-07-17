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
using System.Web.Http.Results;

namespace Cqrs.WebApi
{
	/// <summary>
	/// A <see cref="ApiController"/> that expects the <see cref="ISingleSignOnToken.Token"/> to be sent as a <see cref="HttpHeaders"/> with a key of "X-Token", in accordance with OAuth specifications
	/// </summary>
	/// <remarks>
	/// See https://www.asp.net/web-api/overview/getting-started-with-aspnet-web-api/creating-api-help-pages for details on adding WebApi Help Pages.
	/// </remarks>
	public abstract class CqrsApiController
		: ApiController
	{
		protected CqrsApiController(ILogger logger, ICorrelationIdHelper correlationIdHelper)
		{
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
		}

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		protected virtual string GetToken()
		{
			string token = null;
			IEnumerable<string> tokenValue;
			if (Request.Headers.TryGetValues("X-Token", out tokenValue))
				token = tokenValue.First();
			else
			{
				CookieHeaderValue cookie = Request.Headers.GetCookies("X-Token").FirstOrDefault();
				if (cookie != null)
					token = cookie["X-Token"].Value;
			}

			return token;
		}

		protected virtual IServiceRequest<TSingleSignOnToken> CreateRequest<TSingleSignOnToken>()
			where TSingleSignOnToken : ISingleSignOnToken, new()
		{
			return new ServiceRequest<TSingleSignOnToken>
			{
				AuthenticationToken = CreateAuthenticationToken<TSingleSignOnToken>(),
				CorrelationId = CorrelationIdHelper.GetCorrelationId()
			};
		}

		protected virtual IServiceRequestWithData<TSingleSignOnToken, TParameters> CreateRequestWithData<TSingleSignOnToken, TParameters>(Func<TParameters> createParameterDelegate = null)
			where TSingleSignOnToken : ISingleSignOnToken, new()
			where TParameters : new()
		{
			return new ServiceRequestWithData<TSingleSignOnToken, TParameters>
			{
				AuthenticationToken = CreateAuthenticationToken<TSingleSignOnToken>(),
				CorrelationId = CorrelationIdHelper.GetCorrelationId(),
				Data = createParameterDelegate == null ? CreateParameter<TParameters>() : createParameterDelegate()
			};
		}

		protected virtual TSingleSignOnToken CreateAuthenticationToken<TSingleSignOnToken>()
			where TSingleSignOnToken : ISingleSignOnToken, new()
		{
			return new TSingleSignOnToken
			{
				DateIssued = GetDateTokenIssued(),
				Token = GetToken(),
				TimeOfExpiry = GetTokenTimeOfExpiry()
			};
		}

		protected virtual TParameters CreateParameter<TParameters>()
			where TParameters : new()
		{
			return new TParameters();
		}

		protected virtual DateTime GetDateTokenIssued()
		{
			return default(DateTime);
		}

		protected virtual DateTime GetTokenTimeOfExpiry()
		{
			return default(DateTime);
		}

		protected virtual HttpResponseMessage CompleteResponse<TServiceResponse>(TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponse
		{
			serviceResponse.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			var response = new HttpResponseMessage();

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
		protected CqrsApiController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			: base(logger, correlationIdHelper)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected override string GetToken()
		{
			TAuthenticationToken token = AuthenticationTokenHelper.GetAuthenticationToken();
			if (token != null)
				return token.ToString();
			return null;
		}

		protected virtual IServiceRequest<TAuthenticationToken> CreateRequest()
		{
			TAuthenticationToken token = AuthenticationTokenHelper.GetAuthenticationToken();
			return new ServiceRequest<TAuthenticationToken>
			{
				AuthenticationToken = token,
				CorrelationId = CorrelationIdHelper.GetCorrelationId()
			};
		}

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