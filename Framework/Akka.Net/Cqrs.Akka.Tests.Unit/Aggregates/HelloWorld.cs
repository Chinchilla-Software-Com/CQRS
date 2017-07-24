#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Akka.Tests.Unit.Events;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Domain;

namespace Cqrs.Akka.Tests.Unit.Aggregates
{
	public class HelloWorld : AkkaAggregateRoot<Guid>
	{
		public Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		public bool IsLogicallyDeleted {get; set;}

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

		public HelloWorld(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this(dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		public virtual void SayHello(SayHelloWorldCommand command)
		{
			SayHello();
		}

		public virtual void ReplyToHelloWorld(ReplyToHelloWorldCommand command)
		{
			ReplyToHelloWorld();
		}

		public virtual void EndConversation(EndConversationCommand command)
		{
			EndConversation();
		}

		public virtual void SayHello()
		{
			ApplyChange(new HelloWorldSaid { Id = Id });
		}

		public virtual void ReplyToHelloWorld()
		{
			ApplyChange(new HelloWorldRepliedTo { Id = Id });
		}

		public virtual void EndConversation()
		{
			ApplyChange(new ConversationEnded { Id = Id });
		}
	}
}