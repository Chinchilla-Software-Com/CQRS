namespace Chat.MicroServices.Conversations.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Repositories;
	using Repositories.Queries.Strategies;
	using System;

	public class ConversationDeletedEventHandler : IEventHandler<Guid, ConversationDeleted>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		public ConversationDeletedEventHandler(IAutomapHelper automapHelper, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository, IQueryFactory queryFactory)
		{
			AutomapHelper = automapHelper;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
			QueryFactory = queryFactory;
		}

		#region Implementation of IEventHandler<in ConversationDeleted>

		public void Handle(ConversationDeleted @event)
		{
			// As this is a delete of an existing conversation, load the existing conversation
			ConversationSummaryEntity conversationSummary = ConversationSummaryRepository.Load(@event.Rsn);
			// This will logically delete the conversation so it can be retrieved in a list of deleted conversations to be un-deleted.
			ConversationSummaryRepository.Delete(conversationSummary);

			// Update all message projections
			// Define Query
			ICollectionResultQuery<MessageQueryStrategy, MessageEntity> query = QueryFactory.CreateNewCollectionResultQuery<MessageQueryStrategy, MessageEntity>();

			query.QueryStrategy.WithConversationRsn(@event.Rsn);
			query.QueryStrategy.OrderByDatePosted();

			// Retrieve Data
			query = MessageRepository.Retrieve(query);

			foreach (MessageEntity messageEntity in query.Result)
			{
				// This will logically delete the message so it can be retrieved in a list of deleted messages to be un-deleted.
				MessageRepository.Delete(messageEntity);
			}
		}

		#endregion
	}
}