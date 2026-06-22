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
using Cqrs.Configuration;
using Cqrs.Domain;

namespace Cqrs.Akka.Tests.Unit.Aggregates
{
	/// <summary>
	/// An Akka.Net actor based <see cref="IAggregateRoot{TAuthenticationToken}"/> that represents a conversation.
	/// </summary>
	public class HelloWorld : AkkaAggregateRoot<Guid>
	{
		/// <summary>
		/// The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/>.
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

// ReSharper disable UnusedMember.Local
		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		private HelloWorld()
			: base(null, null, null, null, null)
		{
			Receive<SayHelloWorldCommand>(command => Execute(SayHello, command));
			Receive<ReplyToHelloWorldCommand>(command => Execute(ReplyToHelloWorld, command));
			Receive<EndConversationCommand>(command => Execute(EndConversation, command));
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		private HelloWorld(IDependencyResolver dependencyResolver, ILogger logger)
			: this()
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
			UnitOfWork = DependencyResolver.Resolve<IUnitOfWork<Guid>>();
			Repository = DependencyResolver.Resolve<IAkkaAggregateRepository<Guid>>();
			CorrelationIdHelper = DependencyResolver.Resolve<ICorrelationIdHelper>();
			AuthenticationTokenHelper = DependencyResolver.Resolve<IAuthenticationTokenHelper<Guid>>();
		}
// ReSharper restore UnusedMember.Local

		/// <summary>
		/// Instantiates a new instance of <see cref="HelloWorld"/>.
		/// </summary>
		public HelloWorld(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this(dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		/// <summary>
		/// Raises a <see cref="HelloWorldSaid"/>.
		/// </summary>
		public virtual void SayHello(SayHelloWorldCommand command)
		{
			SayHello();
		}

		/// <summary>
		/// Raises a <see cref="HelloWorldRepliedTo"/>.
		/// </summary>
		public virtual void ReplyToHelloWorld(ReplyToHelloWorldCommand command)
		{
			ReplyToHelloWorld();
		}

		/// <summary>
		/// Raises a <see cref="ConversationEnded"/>.
		/// </summary>
		public virtual void EndConversation(EndConversationCommand command)
		{
			EndConversation();
		}

		/// <summary>
		/// Raises a <see cref="HelloWorldSaid"/>.
		/// </summary>
		public virtual void SayHello()
		{
			ApplyChange(new HelloWorldSaid { Id = Id });
		}

		/// <summary>
		/// Raises a <see cref="HelloWorldRepliedTo"/>.
		/// </summary>
		public virtual void ReplyToHelloWorld()
		{
			ApplyChange(new HelloWorldRepliedTo { Id = Id });
		}

		/// <summary>
		/// Raises a <see cref="ConversationEnded"/>.
		/// </summary>
		public virtual void EndConversation()
		{
			ApplyChange(new ConversationEnded { Id = Id });
		}
	}
}