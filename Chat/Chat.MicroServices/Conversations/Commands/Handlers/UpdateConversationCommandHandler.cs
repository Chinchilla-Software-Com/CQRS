namespace Chat.MicroServices.Conversations.Commands.Handlers
{
	using Cqrs.Domain;
	using System;
	using Cqrs.Commands;
	using Cqrs.Configuration;
	using cdmdotnet.Logging;

	/// <summary>
	/// Responds to any <see cref="UpdateConversation"/>, loads the existing <see cref="Conversation"/> instance and passes the contents of the <see cref="UpdateConversation"/> to it.
	/// </summary>
	public class UpdateConversationCommandHandler : ICommandHandler<Guid, UpdateConversation>
	{
		protected IUnitOfWork<Guid> UnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		public UpdateConversationCommandHandler(IUnitOfWork<Guid> unitOfWork, IDependencyResolver dependencyResolver, ILogger logger)
		{
			UnitOfWork = unitOfWork;
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		#region Implementation of ICommandHandler<in UpdateConversation>

		public void Handle(UpdateConversation command)
		{
			// As this is a request to update the name of an existing conversation, load the existing conversation (which will automatically add it to the UnitOfWork)
			Conversation item = UnitOfWork.Get<Conversation>(command.Rsn);
			// Request the name be updated
			item.Update(command.Name);
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}