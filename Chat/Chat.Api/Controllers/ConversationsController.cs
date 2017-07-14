namespace Chat.Api.Controllers
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Cqrs.WebApi;
	using MicroServices.Conversations.Entities;
	using MicroServices.Conversations.Repositories;
	using MicroServices.Conversations.Repositories.Queries.Strategies;
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Formatting;
	using System.Web.Http;

	[RoutePrefix("Conversation")]
	public class ConversationsController : CqrsApiController<Guid>
	{
		public ConversationsController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IQueryFactory queryFactory, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository)
			: base(logger, correlationIdHelper, authenticationTokenHelper)
		{
			QueryFactory = queryFactory;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
		}

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		[Route("")]
		[HttpGet]
		public virtual HttpResponseMessage Get()
		{
			// Define Query
			ICollectionResultQuery<ConversationSummaryQueryStrategy, ConversationSummaryEntity> query = QueryFactory.CreateNewCollectionResultQuery<ConversationSummaryQueryStrategy, ConversationSummaryEntity>();

			query.QueryStrategy.WithNoDeletedConversations();

			// Retrieve Data
			query = ConversationSummaryRepository.Retrieve(query);
			IEnumerable<ConversationSummaryEntity> queryResults = query.Result;

			var responseData = new ServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = queryResults
			};

			// Complete the response
			HttpResponseMessage response = CompleteResponse<ServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>>, IEnumerable<ConversationSummaryEntity>>(responseData);

			return response;
		}

		[Route("{conversationRsn:guid}")]
		[HttpGet]
		public virtual HttpResponseMessage GetMessages(Guid conversationRsn)
		{
			// Define Query
			ICollectionResultQuery<MessageQueryStrategy, MessageEntity> query = QueryFactory.CreateNewCollectionResultQuery<MessageQueryStrategy, MessageEntity>();

			query.QueryStrategy.WithConversationRsn(conversationRsn);
			query.QueryStrategy.OrderByDatePosted();

			// Retrieve Data
			query = MessageRepository.Retrieve(query);
			IEnumerable<MessageEntity> queryResults = query.Result;

			var responseData = new ServiceResponseWithResultData<IEnumerable<MessageEntity>>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = queryResults
			};

			// Complete the response
			HttpResponseMessage response = CompleteResponse<ServiceResponseWithResultData<IEnumerable<MessageEntity>>, IEnumerable<MessageEntity>>(responseData);

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