namespace $safeprojectname$.Conversations.Commands.Handlers
{
	using Cqrs.Domain;
	using System;
	using Cqrs.Commands;
	using Cqrs.Configuration;
	using cdmdotnet.Logging;

	/// <summary>
	/// Responds to any <see cref="StartConversation"/>, creates new <see cref="Conversation"/> instance and passes the contents of the <see cref="StartConversation"/> to it.
	/// </summary>
	public class StartConversationCommandHandler : ICommandHandler<Guid, StartConversation>
	{
		protected IUnitOfWork<Guid> UnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		public StartConversationCommandHandler(IUnitOfWork<Guid> unitOfWork, IDependencyResolver dependencyResolver, ILogger logger)
		{
			UnitOfWork = unitOfWork;
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		#region Implementation of ICommandHandler<in StartConversation>

		public void Handle(StartConversation command)
		{
			// As this is a request to create an conversation, create a new conversation and add it to the UnitOfWork
			Conversation item = new Conversation(DependencyResolver, Logger, Guid.NewGuid());
			UnitOfWork.Add(item);
			// Request the conversation be started
			item.Start(command.Name);
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}