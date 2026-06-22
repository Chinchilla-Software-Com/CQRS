#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Akka.Tests.Unit.Events;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Domain;

namespace Cqrs.Akka.Tests.Unit.Sagas
{
	/// <summary>
	/// A <see cref="ISaga{TAuthenticationToken}"/> that acts as a process manager responding to several events and raising a command when a certain criteria is met.
	/// </summary>
	public class ConversationReportProcessManager : AkkaSaga<Guid>
	{
		/// <summary>
		/// The <see cref="ISaga{TAuthenticationToken}.Id"/>
		/// </summary>
		public Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		/// <summary>
		/// Indicates if this <see cref="ISaga{TAuthenticationToken}"/> has been deleted.
		/// </summary>
		public bool IsDeleted { get; set; }

		/// <summary>
		/// The <see cref="IDependencyResolver"/> that resolves things.
		/// </summary>
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

		/// <summary>
		/// Instantiates a new instance of <see cref="ConversationReportProcessManager"/>.
		/// </summary>
		public ConversationReportProcessManager(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this(dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		/// <summary>
		/// Responds to the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="event">The <see cref="HelloWorldSaid"/> to respond to or "handle"</param>
		public virtual void Handle(HelloWorldSaid @event)
		{
			ApplyChange(@event);
			GenerateCommand();
		}

		/// <summary>
		/// Responds to the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="event">The <see cref="HelloWorldRepliedTo"/> to respond to or "handle"</param>
		public virtual void Handle(HelloWorldRepliedTo @event)
		{
			ApplyChange(@event);
			GenerateCommand();
		}

		/// <summary>
		/// Responds to the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="event">The <see cref="ConversationEnded"/> to respond to or "handle"</param>
		public virtual void Handle(ConversationEnded @event)
		{
			ApplyChange(@event);
			GenerateCommand();
		}

		/// <summary>
		/// Generates and publishes a <see cref="ICommand{TAuthenticationToken}"/>.
		/// </summary>
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

		/// <summary>
		/// Applies the <paramref name="event"/> to itself.
		/// </summary>
		/// <param name="event">The <see cref="HelloWorldSaid"/> to apply</param>
		public virtual void Apply(HelloWorldSaid @event)
		{
			HelloWorldWasSaid = true;
		}

		/// <summary>
		/// Applies the <paramref name="event"/> to itself.
		/// </summary>
		/// <param name="event">The <see cref="HelloWorldRepliedTo"/> to apply</param>
		public virtual void Apply(HelloWorldRepliedTo @event)
		{
			HelloWorldWasRepliedTo = true;
		}

		/// <summary>
		/// Applies the <paramref name="event"/> to itself.
		/// </summary>
		/// <param name="event">The <see cref="ConversationEnded"/> to apply</param>
		public virtual void Apply(ConversationEnded @event)
		{
			ConversationWasEnded = true;
		}
	}
}