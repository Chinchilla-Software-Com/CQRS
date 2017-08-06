namespace $safeprojectname$.Conversations.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Factories;
	using Queries.Strategies;

	public class ConversationSummaryRepository : Repository<ConversationSummaryQueryStrategy, ConversationSummaryQueryStrategyBuilder, ConversationSummaryEntity>, IConversationSummaryRepository
	{
		public ConversationSummaryRepository(IDomainDataStoreFactory dataStoreFactory, ConversationSummaryQueryStrategyBuilder conversationSummaryQueryBuilder)
			: base(dataStoreFactory.GetConversationSummaryDataStore, conversationSummaryQueryBuilder)
		{
		}
	}
}