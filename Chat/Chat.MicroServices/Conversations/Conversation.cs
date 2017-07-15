namespace Chat.MicroServices.Conversations
{
	using System;
	using cdmdotnet.Logging;
	using Cqrs.Configuration;
	using Cqrs.Domain;
	using Events;

	public class Conversation : AggregateRoot<Guid>
	{
		protected Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>.
		/// </summary>
		private Conversation()
		{
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/> to operate with new instances.
		/// </summary>
		private Conversation(IDependencyResolver dependencyResolver, ILogger logger)
			: this()
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/> to operate with existing instances.
		/// </summary>
		public Conversation(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this (dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		protected int CurrentMessageCount { get; set; }

		protected string Name { get; set; }

		/// <summary>
		/// Start a new conversation
		/// </summary>
		/// <param name="name">The name of the conversation.</param>
		public virtual void Start(string name)
		{
			ApplyChange(new ConversationStarted(Rsn, Name));
		}

		/// <summary>
		/// Post the provided <paramref name="comment"/>
		/// </summary>
		/// <param name="userRsn">The user who is posting the comment.</param>
		/// <param name="userName">The name of the user who is posting the comment.</param>
		/// <param name="comment">The content of the comment being posted.</param>
		public virtual void PostComment(Guid userRsn, string userName, string comment)
		{
			ApplyChange(new CommentPosted(Guid.NewGuid(), Rsn, Name, userRsn, userName, comment, DateTime.UtcNow, CurrentMessageCount + 1));
		}

		/// <summary>
		/// This will set the <see cref="Name"/>.
		/// </summary>
		protected virtual void Apply(ConversationStarted @event)
		{
			// One of the event handlers adds a system generated "Welcome" message into each conversation.
			CurrentMessageCount++;
			Name = @event.Name;
		}

		/// <summary>
		/// This will increment the <see cref="CurrentMessageCount"/>.
		/// </summary>
		protected virtual void Apply(CommentPosted @event)
		{
			CurrentMessageCount++;
		}
	}
}