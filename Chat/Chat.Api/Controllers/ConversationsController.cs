namespace Chat.Api.Controllers
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Commands;
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Cqrs.WebApi;
	using Helpers;
	using MicroServices.Conversations.Commands;
	using MicroServices.Conversations.Entities;
	using MicroServices.Conversations.Repositories;
	using MicroServices.Conversations.Repositories.Queries.Strategies;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http;

	[RoutePrefix("Conversation")]
	public class ConversationsController : CqrsApiController<Guid>
	{
		public ConversationsController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IQueryFactory queryFactory, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository, IAuthenticationHelper authenticationHelper, ICommandPublisher<Guid> commandPublisher)
			: base(logger, correlationIdHelper, authenticationTokenHelper)
		{
			QueryFactory = queryFactory;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
			AuthenticationHelper = authenticationHelper;
			CommandPublisher = commandPublisher;
		}

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		protected IAuthenticationHelper AuthenticationHelper { get; private set; }

		protected ICommandPublisher<Guid> CommandPublisher { get; private set; }

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
			IEnumerable<MessageEntity> queryResults = query.Result.ToList();

			var responseData = new ServiceResponseWithResultData<IEnumerable<MessageEntity>>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = queryResults
			};

			// Complete the response
			HttpResponseMessage response = CompleteResponse<ServiceResponseWithResultData<IEnumerable<MessageEntity>>, IEnumerable<MessageEntity>>(responseData);

			if (!queryResults.Any())
				response.StatusCode = HttpStatusCode.NotFound;

			return response;
		}

		[Route("{conversationRsn:guid}")]
		[HttpPut]
		public virtual IHttpActionResult PostComment(Guid conversationRsn, [FromBody]string comment)
		{
			var responseData = new ServiceResponse
			{
				State = ServiceResponseStateType.FailedValidation
			};

			if (string.IsNullOrWhiteSpace(comment))
				return CompleteResponse(responseData);

			string userName;
			try
			{
				userName = AuthenticationHelper.GetCurrentUsersName();
			}
			catch (InvalidOperationException)
			{
				responseData.State = ServiceResponseStateType.FailedAuthentication;
				return CompleteResponse(responseData);
			}

			var command = new PostComment
			{
				UserRsn = AuthenticationHelper.GetCurrentUser(),
				UserName = userName,
				ConversationRsn = conversationRsn,
				Comment = comment
			};

			try
			{
				CommandPublisher.Publish(command);

				responseData.State = ServiceResponseStateType.Succeeded;
			}
			catch (Exception)
			{
				responseData.State = ServiceResponseStateType.Unknown;
			}

			// Complete the response
			return CompleteResponse(responseData);
		}
	}
}