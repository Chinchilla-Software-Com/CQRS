namespace Chat.MicroServices.Conversations.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Queries.Strategies;

	public interface IMessageRepository : IRepository<MessageQueryStrategy, MessageEntity>
	{
	}
}