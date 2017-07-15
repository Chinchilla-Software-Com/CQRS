namespace Chat.MicroServices.Conversations.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Authentication;
	using Cqrs.Events;
	using Entities;
	using Repositories;
	using System;

	public class ConversationStartedEventHandler : IEventHandler<Guid, ConversationStarted>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		protected IAuthenticationTokenHelper<Guid> AuthenticationTokenHelper { get; private set; }

		public ConversationStartedEventHandler(IAutomapHelper automapHelper, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository, IAuthenticationTokenHelper<Guid> authenticationTokenHelper)
		{
			AutomapHelper = automapHelper;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		#region Implementation of IEventHandler<in ConversationStarted>

		public void Handle(ConversationStarted @event)
		{
			var conversationSummary = new ConversationSummaryEntity
			{
				Rsn = @event.Rsn,
				Name = @event.Name,
				LastUpdatedDate = @event.TimeStamp.UtcDateTime,
				// Below we add the first message
				MessageCount = 1
			};
			// As this is the creation of a new conversation, pass the entity to the Repository for creation and persisted
			ConversationSummaryRepository.Create(conversationSummary);

			MessageEntity entity = new MessageEntity
			{
				Rsn = @event.Rsn,
				ConversationRsn = @event.Rsn,
				ConversationName = @event.Name,
				UserRsn = AuthenticationTokenHelper.GetAuthenticationToken(),
				UserName = "System",
				Content = "Welcome!",
				DatePosted = @event.TimeStamp.UtcDateTime
			};

			// As this is the creation of a new comment, pass the entity to the Repository for creation and persisted
			MessageRepository.Create(entity);
		}

		#endregion
	}
}