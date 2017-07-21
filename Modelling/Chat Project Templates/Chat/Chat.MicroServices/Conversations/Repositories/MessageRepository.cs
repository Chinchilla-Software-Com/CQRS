namespace $safeprojectname$.Conversations.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Factories;
	using Queries.Strategies;

	public class MessageRepository : Repository<MessageQueryStrategy, MessageQueryStrategyBuilder, MessageEntity>, IMessageRepository
	{
		public MessageRepository(IDomainDataStoreFactory dataStoreFactory, MessageQueryStrategyBuilder messageQueryBuilder)
			: base(dataStoreFactory.GetMessageDataStore, messageQueryBuilder)
		{
		}
	}
}