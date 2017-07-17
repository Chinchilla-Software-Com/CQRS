namespace Chat.MicroServices.Conversations.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Entities;
	using Repositories;
	using System;

	public class ConversationStartedEventHandler : IEventHandler<Guid, ConversationStarted>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		public ConversationStartedEventHandler(IAutomapHelper automapHelper, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository)
		{
			AutomapHelper = automapHelper;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
		}

		#region Implementation of IEventHandler<in ConversationStarted>

		public void Handle(ConversationStarted @event)
		{
			var conversationSummary = new ConversationSummaryEntity
			{
				Rsn = @event.Rsn,
				Name = @event.Name,
				LastUpdatedDate = @event.TimeStamp.UtcDateTime,
				MessageCount = 0
			};
			// As this is the creation of a new conversation, pass the entity to the Repository for creation and persisted
			ConversationSummaryRepository.Create(conversationSummary);
		}

		#endregion
	}
}