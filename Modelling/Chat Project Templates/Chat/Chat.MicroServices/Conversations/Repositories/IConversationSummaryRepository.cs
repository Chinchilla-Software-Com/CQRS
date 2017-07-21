namespace $safeprojectname$.Conversations.Repositories
{
	using Cqrs.Repositories;
	using Entities;
	using Queries.Strategies;

	public interface IConversationSummaryRepository : IRepository<ConversationSummaryQueryStrategy, ConversationSummaryEntity>
	{
	}
}