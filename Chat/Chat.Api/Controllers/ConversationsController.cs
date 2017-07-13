namespace Chat.Api.Controllers
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Services;
	using Cqrs.WebApi;
	using Models;
	using System;
	using System.Net.Http;
	using System.Net.Http.Formatting;
	using System.Net.Http.Headers;
	using System.Web.Http;

	[RoutePrefix("Conversation")]
	public class ConversationsController : CqrsApiController<Guid>
	{
		public ConversationsController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper)
			: base(logger, correlationIdHelper, authenticationTokenHelper)
		{
		}

		[Route("")]
		[HttpPost]
		public virtual IServiceResponseWithResultData<dynamic> Get()
		{
			var responseData = new ServiceResponseWithResultData<dynamic>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = new
				{
					Results = new []
					{
						new
						{
							Rsn = Guid.NewGuid(),
							Name = "Chat with AJ",
							Started = DateTime.Now.AddDays(-3),
							LastUpdated = DateTime.Now,
							MessageCount = 42
						},
						new
						{
							Rsn = Guid.NewGuid(),
							Name = "Chat with Matt",
							Started = DateTime.Now.AddDays(-5),
							LastUpdated = DateTime.Now,
							MessageCount = 1
						}

					}
				}
			};

			CompleteResponse(responseData);

			return responseData;
		}
	}
}