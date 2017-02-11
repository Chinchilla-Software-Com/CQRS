using System;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Events;
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
			: base(null, null, null)
		{
			Receive<SayHelloParameters>(parameters => Execute(SayHello, parameters));
			Receive<HelloWorldReplyParameters>(parameters => Execute(HelloWorldReply, parameters));
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
			Repository = DependencyResolver.Resolve<IAkkaRepository<Guid>>();
		}
// ReSharper restore UnusedMember.Local

		public HelloWorld(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this(dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		public virtual void SayHello(SayHelloParameters parameter)
		{
			SayHello();
		}

		public virtual void HelloWorldReply(HelloWorldReplyParameters parameter)
		{
			HelloWorldReply();
		}

		public virtual void SayHello()
		{
			ApplyChange(new HelloWorldSaid { Id = Id });
		}

		public virtual void HelloWorldReply()
		{
			ApplyChange(new HelloWorldReplied { Id = Id });
		}
	}

	public class SayHelloParameters { }

	public class HelloWorldReplyParameters { }
}