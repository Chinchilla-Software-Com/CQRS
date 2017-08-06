namespace $safeprojectname$.Conversations.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Entities;
	using Repositories;
	using System;

	public class CommentPostedEventHandler : IEventHandler<Guid, CommentPosted>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		public CommentPostedEventHandler(IAutomapHelper automapHelper, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository)
		{
			AutomapHelper = automapHelper;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
		}

		#region Implementation of IEventHandler<in CommentPosted>

		public void Handle(CommentPosted @event)
		{
			// Update the message count first
			// As this is an update of an existing conversation, load the existing conversation
			ConversationSummaryEntity conversationSummary = ConversationSummaryRepository.Load(@event.ConversationRsn);
			// Update the message count from the aggregate
			conversationSummary.MessageCount = @event.CurrentMessageCount;
			conversationSummary.LastUpdatedDate = @event.DatePosted;
			// As this is an update of an existing conversation, pass the updated entity to the Repository to be updated and persisted
			ConversationSummaryRepository.Update(conversationSummary);

			MessageEntity messageEntity = new MessageEntity
			{
				Rsn = @event.MessageRsn,
				ConversationRsn = @event.ConversationRsn,
				ConversationName = @event.ConversationName,
				UserRsn = @event.UserRsn,
				UserName = @event.UserName,
				Content = @event.Comment,
				DatePosted = @event.DatePosted
			};

			// As this is the creation of a new comment, pass the entity to the Repository for creation and persisted
			MessageRepository.Create(messageEntity);
		}

		#endregion
	}
}