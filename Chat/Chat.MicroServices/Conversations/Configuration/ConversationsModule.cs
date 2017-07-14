namespace Chat.MicroServices.Conversations.Configuration
{
	using Ninject;
	using Repositories;

	public class ConversationsModule
	{
		/// <summary>
		/// Load all required Repositories
		/// </summary>
		public virtual void LoadRepositories(IKernel kernel)
		{
			kernel.Bind<IConversationSummaryRepository>()
				.To<ConversationSummaryRepository>()
				.InSingletonScope();
			kernel.Bind<IMessageRepository>()
				.To<MessageRepository>()
				.InSingletonScope();
		}
	}
}