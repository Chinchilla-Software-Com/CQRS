namespace $safeprojectname$.Conversations.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Repositories;
	using Repositories.Queries.Strategies;
	using System;

	public class ConversationUpdatedEventHandler : IEventHandler<Guid, ConversationUpdated>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IConversationSummaryRepository ConversationSummaryRepository { get; private set; }

		protected IMessageRepository MessageRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		public ConversationUpdatedEventHandler(IAutomapHelper automapHelper, IConversationSummaryRepository conversationSummaryRepository, IMessageRepository messageRepository, IQueryFactory queryFactory)
		{
			AutomapHelper = automapHelper;
			ConversationSummaryRepository = conversationSummaryRepository;
			MessageRepository = messageRepository;
			QueryFactory = queryFactory;
		}

		#region Implementation of IEventHandler<in ConversationUpdated>

		public void Handle(ConversationUpdated @event)
		{
			// As this is an update of an existing conversation, load the existing conversation
			ConversationSummaryEntity conversationSummary = ConversationSummaryRepository.Load(@event.Rsn);
			// Update the name
			conversationSummary.Name = @event.Name;
			// As this is an update of an existing conversation, pass the updated entity to the Repository to be updated and persisted
			ConversationSummaryRepository.Update(conversationSummary);

			// Update all message projections
			// Define Query
			ICollectionResultQuery<MessageQueryStrategy, MessageEntity> query = QueryFactory.CreateNewCollectionResultQuery<MessageQueryStrategy, MessageEntity>();

			query.QueryStrategy.WithConversationRsn(@event.Rsn);
			query.QueryStrategy.OrderByDatePosted();

			// Retrieve Data
			query = MessageRepository.Retrieve(query);

			foreach (MessageEntity messageEntity in query.Result)
			{
				// Update the name
				messageEntity.ConversationName = @event.Name;

				// As this is an update of an existing message, pass the updated entity to the Repository to be updated and persisted
				MessageRepository.Update(messageEntity);
			}
		}

		#endregion
	}
}