namespace Chat.RestAPI.Controllers
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Commands;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Cqrs.WebApi;
	using Helpers;
	using MicroServices.Conversations;
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

	/// <summary>
	/// A WebAPI RESTful service for accessing and modifying <see cref="Conversation">conversations</see>.
	/// </summary>
	[RoutePrefix("Conversation")]
	public class ConversationsController : CqrsApiController<Guid>
	{
		public ConversationsController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IConfigurationManager configurationManager, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IQueryFactory queryFactory, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository, IAuthenticationHelper authenticationHelper, ICommandPublisher<Guid> commandPublisher)
			: base(logger, correlationIdHelper, configurationManager, authenticationTokenHelper)
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

		/// <summary>
		/// Get all <see cref="ConversationSummaryEntity">conversations</see>.
		/// </summary>
		[Route("")]
		[HttpGet]
		public virtual HttpResponseMessage<IEnumerable<ConversationSummaryEntity>> Get()
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
			HttpResponseMessage response = CompleteResponse(responseData);

			return response;
		}

		/// <summary>
		/// Get all <see cref="MessageEntity">messages</see> for the provided <paramref name="conversationRsn"/>.
		/// </summary>
		/// <param name="conversationRsn">The conversation to get message for.</param>
		[Route("{conversationRsn:guid}/messages")]
		[HttpGet]
		public virtual HttpResponseMessage<IEnumerable<MessageEntity>> GetMessages(Guid conversationRsn)
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
			HttpResponseMessage response = CompleteResponse(responseData);

			if (!queryResults.Any())
				response.StatusCode = HttpStatusCode.NotFound;

			return response;
		}

		/// <summary>
		/// Post a comment to the conversation.
		/// </summary>
		/// <param name="conversationRsn">The conversation to post the message to.</param>
		/// <param name="comment">The content of the comment being posted.</param>
		[Route("{conversationRsn:guid}/messages")]
		[HttpPost]
		public virtual HttpResponseMessage PostComment(Guid conversationRsn, [FromBody]string comment)
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

		/// <summary>
		/// Start a new conversation
		/// </summary>
		/// <param name="name">The name of the conversation to start</param>
		[Route("")]
		[HttpPut]
		public virtual HttpResponseMessage StartConversation([FromBody]string name)
		{
			var responseData = new ServiceResponse
			{
				State = ServiceResponseStateType.FailedValidation
			};

			if (string.IsNullOrWhiteSpace(name))
				return CompleteResponse(responseData);

			var command = new StartConversation
			{
				Name = name
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

		/// <summary>
		/// Update the name of an existing conversation.
		/// </summary>
		/// <param name="conversationRsn">The conversation to update.</param>
		/// <param name="name">The new name of the conversation</param>
		[Route("{conversationRsn:guid}")]
		[HttpPatch]
		public virtual HttpResponseMessage UpdateConversation(Guid conversationRsn, [FromBody]string name)
		{
			var responseData = new ServiceResponse();

			if (string.IsNullOrWhiteSpace(name))
				return CompleteResponse(responseData);

			var command = new UpdateConversation
			{
				Rsn = conversationRsn,
				Name = name
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

		/// <summary>
		/// Delete an existing conversation.
		/// </summary>
		/// <param name="conversationRsn">The conversation to delete.</param>
		[Route("{conversationRsn:guid}")]
		[HttpDelete]
		public virtual HttpResponseMessage DeleteConversation(Guid conversationRsn)
		{
			var responseData = new ServiceResponse();

			var command = new DeleteConversation
			{
				Rsn = conversationRsn
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