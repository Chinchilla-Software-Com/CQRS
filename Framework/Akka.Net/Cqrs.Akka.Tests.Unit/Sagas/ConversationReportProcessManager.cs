using System;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Akka.Tests.Unit.Events;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Sagas
{
	public class ConversationReportProcessManagerEventHandlers
		: IEventHandler<Guid, HelloWorldSaid>
		, IEventHandler<Guid, HelloWorldRepliedTo>
		, IEventHandler<Guid, ConversationEnded>
	{
		/// <summary>
		/// Instantiates the <see cref="ConversationReportProcessManagerEventHandlers"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public ConversationReportProcessManagerEventHandlers(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		public void Handle(HelloWorldRepliedTo message)
		{
			IActorRef item = AggregateResolver.ResolveActor<ConversationReportProcessManager>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion

		#region Implementation of IMessageHandler<in HelloWorldSaid>

		public void Handle(HelloWorldSaid message)
		{
			IActorRef item = AggregateResolver.ResolveActor<ConversationReportProcessManager>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion

		#region Implementation of IMessageHandler<in ConversationEnded>

		public void Handle(ConversationEnded message)
		{
			IActorRef item = AggregateResolver.ResolveActor<ConversationReportProcessManager>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion
	}

	public class ConversationReportProcessManager : AkkaSaga<Guid>
	{
		public Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		public bool IsLogicallyDeleted {get; set;}

		protected IDependencyResolver DependencyResolver { get; private set; }

		private bool HelloWorldWasSaid { get; set; }

		private bool HelloWorldWasRepliedTo { get; set; }

		private bool ConversationWasEnded { get; set; }

// ReSharper disable UnusedMember.Local
		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		private ConversationReportProcessManager()
			: base(null, null, null, null, null, null)
		{
			Receive<HelloWorldSaid>(@event => Execute(Handle, @event));
			Receive<HelloWorldRepliedTo>(@event => Execute(Handle, @event));
			Receive<ConversationEnded>(@event => Execute(Handle, @event));
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		private ConversationReportProcessManager(IDependencyResolver dependencyResolver, ILogger logger)
			: this()
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
			CommandPublisher = DependencyResolver.Resolve<ICommandPublisher<Guid>>();
			UnitOfWork = DependencyResolver.Resolve<ISagaUnitOfWork<Guid>>();
			Repository = DependencyResolver.Resolve<IAkkaSagaRepository<Guid>>();
			CorrelationIdHelper = DependencyResolver.Resolve<ICorrelationIdHelper>();
			AuthenticationTokenHelper = DependencyResolver.Resolve<IAuthenticationTokenHelper<Guid>>();
		}
// ReSharper restore UnusedMember.Local

		public ConversationReportProcessManager(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this(dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		public virtual void Handle(HelloWorldSaid @event)
		{
			ApplyChange(@event);
			GenerateCommand();
		}

		public virtual void Handle(HelloWorldRepliedTo @event)
		{
			ApplyChange(@event);
			GenerateCommand();
		}

		public virtual void Handle(ConversationEnded @event)
		{
			ApplyChange(@event);
			GenerateCommand();
		}

		protected virtual void GenerateCommand()
		{
			if (HelloWorldWasSaid && HelloWorldWasRepliedTo && ConversationWasEnded)
			{
				CommandPublisher.Publish
				(
					new UpdateCompletedConversationReportCommand()
				);
			}
		}

		public virtual void Apply(HelloWorldSaid @event)
		{
			HelloWorldWasSaid = true;
		}

		public virtual void Apply(HelloWorldRepliedTo @event)
		{
			HelloWorldWasRepliedTo = true;
		}

		public virtual void Apply(ConversationEnded @event)
		{
			ConversationWasEnded = true;
		}
	}
}