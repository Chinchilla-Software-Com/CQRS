namespace $safeprojectname$.Conversations.Commands.Handlers
{
	using Cqrs.Domain;
	using System;
	using Cqrs.Commands;
	using Cqrs.Configuration;
	using cdmdotnet.Logging;

	/// <summary>
	/// Responds to any <see cref="DeleteConversation"/>, loads the existing <see cref="Conversation"/> instance and passes the contents of the <see cref="DeleteConversation"/> to it.
	/// </summary>
	public class DeleteConversationCommandHandler : ICommandHandler<Guid, DeleteConversation>
	{
		protected IUnitOfWork<Guid> UnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		public DeleteConversationCommandHandler(IUnitOfWork<Guid> unitOfWork, IDependencyResolver dependencyResolver, ILogger logger)
		{
			UnitOfWork = unitOfWork;
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		#region Implementation of ICommandHandler<in DeleteConversation>

		public void Handle(DeleteConversation command)
		{
			// As this is a request to delete an existing conversation, load the existing conversation (which will automatically add it to the UnitOfWork)
			Conversation item = UnitOfWork.Get<Conversation>(command.Rsn);
			// Request the conversation be deleted
			item.Delete();
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}