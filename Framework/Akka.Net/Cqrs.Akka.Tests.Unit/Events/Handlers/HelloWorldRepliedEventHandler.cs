using System;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Akka.Tests.Unit.Aggregates;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	public class HelloWorldRepliedEventHandler
		: IEventHandler<Guid, HelloWorldReplied>
	{
		/// <summary>
		/// Instantiates the <see cref="HelloWorldRepliedEventHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public HelloWorldRepliedEventHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldReplied>

		public void Handle(HelloWorldReplied message)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorldRepliedEventHandlerActor>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion
	}

	public class HelloWorldRepliedEventHandlerActor
		: AkkaEventHandler
	{
		#region Implementation of IMessageHandler<in HelloWorldReplied>

		public void Handle(HelloWorldReplied message)
		{
			UnitTest1.Step2Reached = true;
		}

		#endregion

		public HelloWorldRepliedEventHandlerActor(ILogger logger)
			: base(logger)
		{
			Receive<HelloWorldReplied>(@event => Execute(Handle, @event));
		}
	}
}