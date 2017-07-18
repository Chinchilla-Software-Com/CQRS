namespace Chat.MicroServices.Conversations.Services
{
	using cdmdotnet.Logging;
	using Commands;
	using Cqrs.Authentication;
	using Cqrs.Commands;
	using Cqrs.Events;
	using Cqrs.Repositories.Queries;
	using Cqrs.Services;
	using Entities;
	using Repositories;
	using Repositories.Queries.Strategies;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	/// <summary>
	/// A WCF service for accessing and modifying <see cref="Conversation">conversations</see>.
	/// </summary>
	[DataContract(Namespace = "https://getcqrs.net/Conversations/1001/")]
	public class ConversationService : EventService<Guid>, IConversationService
	{
		public ConversationService(IEventStore<Guid> eventStore, ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IConversationSummaryRepository conversationSummaryRepository, IQueryFactory queryFactory, IMessageRepository messageRepository, ICommandPublisher<Guid> commandPublisher)
			: base(eventStore, logger, correlationIdHelper, authenticationTokenHelper)
		{
			ConversationSummaryRepository = conversationSummaryRepository;
			QueryFactory = queryFactory;
			MessageRepository = messageRepository;
			CommandPublisher = commandPublisher;
		}

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		protected ICommandPublisher<Guid> CommandPublisher { get; private set; }

		/// <summary>
		/// Get all <see cref="ConversationSummaryEntity">conversations</see>.
		/// </summary>
		public virtual IServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>> Get(IServiceRequest<Guid> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

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
			ServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>> response = CompleteResponse(responseData);

			return response;
		}

		/// <summary>
		/// Get all <see cref="MessageEntity">messages</see> for the provided conversation.
		/// </summary>
		public virtual IServiceResponseWithResultData<IEnumerable<MessageEntity>> GetMessages(IServiceRequestWithData<Guid, ConversationParameters> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			Guid conversationRsn = serviceRequest.Data.ConversationRsn;

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
			ServiceResponseWithResultData<IEnumerable<MessageEntity>> response = CompleteResponse(responseData);

			if (!queryResults.Any())
				response.State = ServiceResponseStateType.EntityNotFound;

			return response;
		}

		/// <summary>
		/// Post a comment to the conversation.
		/// </summary>
		public virtual IServiceResponse PostComment(IServiceRequestWithData<Guid, PostCommentParameters> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			var responseData = new ServiceResponse
			{
				State = ServiceResponseStateType.FailedValidation
			};

			if (string.IsNullOrWhiteSpace(serviceRequest.Data.Comment))
				return CompleteResponse(responseData);

			var command = new PostComment
			{
				UserRsn = serviceRequest.Data.UserRsn,
				UserName = serviceRequest.Data.UserName,
				ConversationRsn = serviceRequest.Data.ConversationRsn,
				Comment = serviceRequest.Data.Comment
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
		public virtual IServiceResponse StartConversation(IServiceRequestWithData<Guid, StartConversationParameters> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			var responseData = new ServiceResponse
			{
				State = ServiceResponseStateType.FailedValidation
			};

			if (string.IsNullOrWhiteSpace(serviceRequest.Data.Name))
				return CompleteResponse(responseData);

			var command = new StartConversation
			{
				Name = serviceRequest.Data.Name
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
		public virtual IServiceResponse UpdateConversation(IServiceRequestWithData<Guid, UpdateConversationParameters> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			var responseData = new ServiceResponse();

			if (string.IsNullOrWhiteSpace(serviceRequest.Data.Name))
				return CompleteResponse(responseData);

			var command = new UpdateConversation
			{
				Rsn = serviceRequest.Data.ConversationRsn,
				Name = serviceRequest.Data.Name
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
		public virtual IServiceResponse DeleteConversation(IServiceRequestWithData<Guid, ConversationParameters> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			var responseData = new ServiceResponse();

			var command = new DeleteConversation
			{
				Rsn = serviceRequest.Data.ConversationRsn
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
		/// WCF parameters for all operations on a specific conversation.
		/// </summary>
		public class ConversationParameters
		{
			/// <summary>
			/// The conversation.
			/// </summary>
			public Guid ConversationRsn { get;set; }
		}

		/// <summary>
		/// WCF parameters to post a comment to a conversation.
		/// </summary>
		public class PostCommentParameters : ConversationParameters
		{
			/// <summary>
			/// The content of the comment being posted.
			/// </summary>
			public string Comment { get; set; }

			/// <summary>
			/// The person who posted the comment, which might be different from the logged in user if you have permission to post of other people behalf.
			/// </summary>
			public Guid UserRsn { get; set; }

			/// <summary>
			/// The name of the person who posted the comment, which might be different from the logged in user if you have permission to post of other people behalf.
			/// </summary>
			public string UserName { get; set; }
		}

		/// <summary>
		/// WCF parameters to start a new conversation.
		/// </summary>
		public class StartConversationParameters
		{
			/// <summary>
			/// The name of the conversation.
			/// </summary>
			public string Name { get; set; }
		}

		/// <summary>
		/// WCF parameters to update an existing conversation.
		/// </summary>
		public class UpdateConversationParameters : StartConversationParameters
		{
			/// <summary>
			/// The conversation to update.
			/// </summary>
			public Guid ConversationRsn { get;set; }
		}
	}
}