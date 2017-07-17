namespace Chat.MicroServices.Conversations.Services
{
	using Cqrs.Services;
	using Entities;
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;

	/// <summary>
	/// A WCF contract for accessing and modifying <see cref="Conversation">conversations</see>.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Conversations/1001/")]
	public interface IConversationService : IEventService<Guid>
	{
		/// <summary>
		/// Get all <see cref="ConversationSummaryEntity">conversations</see>.
		/// </summary>
		[OperationContract]
		IServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>> Get(IServiceRequest<Guid> serviceRequest);

		/// <summary>
		/// Get all <see cref="MessageEntity">messages</see> for the provided conversation.
		/// </summary>
		[OperationContract]
		IServiceResponseWithResultData<IEnumerable<MessageEntity>> GetMessages(IServiceRequestWithData<Guid, ConversationService.ConversationParameters> serviceRequest);

		/// <summary>
		/// Post a comment to the conversation.
		/// </summary>
		[OperationContract]
		IServiceResponse PostComment(IServiceRequestWithData<Guid, ConversationService.PostCommentParameters> serviceRequest);

		/// <summary>
		/// Start a new conversation
		/// </summary>
		[OperationContract]
		IServiceResponse StartConversation(IServiceRequestWithData<Guid, ConversationService.StartConversationParameters> serviceRequest);

		/// <summary>
		/// Update the name of an existing conversation.
		/// </summary>
		[OperationContract]
		IServiceResponse UpdateConversation(IServiceRequestWithData<Guid, ConversationService.UpdateConversationParameters> serviceRequest);

		/// <summary>
		/// Delete an existing conversation.
		/// </summary>
		[OperationContract]
		IServiceResponse DeleteConversation(IServiceRequestWithData<Guid, ConversationService.ConversationParameters> serviceRequest);
	}
}