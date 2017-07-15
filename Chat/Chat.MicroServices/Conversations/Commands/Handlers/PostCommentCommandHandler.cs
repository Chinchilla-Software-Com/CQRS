namespace Chat.MicroServices.Conversations.Commands.Handlers
{
	using Cqrs.Domain;
	using System;
	using Cqrs.Commands;
	using Cqrs.Configuration;
	using cdmdotnet.Logging;

	/// <summary>
	/// Responds to any <see cref="PostComment"/>, loads the existing <see cref="Conversation"/> instance and passes the contents of the <see cref="PostComment"/> to it.
	/// </summary>
	public class PostCommentCommandHandler : ICommandHandler<Guid, PostComment>
	{
		protected IUnitOfWork<Guid> UnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		public PostCommentCommandHandler(IUnitOfWork<Guid> unitOfWork, IDependencyResolver dependencyResolver, ILogger logger)
		{
			UnitOfWork = unitOfWork;
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		#region Implementation of ICommandHandler<in PostComment>

		public void Handle(PostComment command)
		{
			// As this is a request to post a new comment to an existing conversation, load the existing conversation (which will automatically add it to the UnitOfWork)
			Conversation item = UnitOfWork.Get<Conversation>(command.ConversationRsn);
			// Request the comment be posted
			item.PostComment(command.UserRsn, command.UserName, command.Comment);
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}