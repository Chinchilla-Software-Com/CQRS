#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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

		protected virtual TServiceResponse CompleteResponse<TServiceResponse>(TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponse
		{
			serviceResponse.CorrelationId = CorrelationIdHelper.GetCorrelationId();
			switch (serviceResponse.State)
			{
				case ServiceResponseStateType.Succeeded:
					StatusCode(HttpStatusCode.OK);
					break;
				case ServiceResponseStateType.FailedAuthentication:
					StatusCode(HttpStatusCode.Forbidden);
					break;
				case ServiceResponseStateType.FailedAuthorisation:
					StatusCode(HttpStatusCode.Unauthorized);
					break;
				case ServiceResponseStateType.FailedValidation:
					StatusCode(HttpStatusCode.PreconditionFailed);
					break;
				case ServiceResponseStateType.FailedWithAFatalException:
					StatusCode(HttpStatusCode.InternalServerError);
					break;
				case ServiceResponseStateType.FailedWithAnUnexpectedException:
					StatusCode(HttpStatusCode.InternalServerError);
					break;
				case ServiceResponseStateType.Unknown:
					StatusCode(HttpStatusCode.BadRequest);
					break;
			}
			return serviceResponse;
		}
	}
}