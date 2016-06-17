using System;
using System.Collections.Generic;
using System.Linq;
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
	public abstract class CqrsApiController : ApiController
	{
		protected CqrsApiController(ICorrelationIdHelper correlationIdHelper)
		{
			CorrelationIdHelper = correlationIdHelper;
		}

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected virtual string GetToken()
		{
			string token = null;
			IEnumerable<string> tokenValue;
			if (Request.Headers.TryGetValues("X-Token", out tokenValue))
				token = tokenValue.First();

			return token;
		}

		protected virtual IServiceRequestWithData<TSingleSignOnToken, TParameters> CreateRequestWithData<TSingleSignOnToken, TParameters>()
			where TSingleSignOnToken : ISingleSignOnToken, new()
			where TParameters : new()
		{
			return new ServiceRequestWithData<TSingleSignOnToken, TParameters>
			{
				AuthenticationToken = CreateAuthenticationToken<TSingleSignOnToken>(),
				CorrelationId = CorrelationIdHelper.GetCorrelationId(),
				Data = CreateParameter<TParameters>()
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
	}
}
