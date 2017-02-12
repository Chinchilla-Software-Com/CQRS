using System;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	public class HelloWorldRepliedToEventHandler
		: IEventHandler<Guid, HelloWorldRepliedTo>
	{
		/// <summary>
		/// Instantiates the <see cref="HelloWorldRepliedToEventHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public HelloWorldRepliedToEventHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		public void Handle(HelloWorldRepliedTo message)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorldRepliedToEventHandlerActor>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion
	}

	public class HelloWorldRepliedToEventHandlerActor
		: AkkaEventHandler
	{
		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		public void Handle(HelloWorldRepliedTo message)
		{
			UnitTest1.Step2Reached = true;
		}

		#endregion

		public HelloWorldRepliedToEventHandlerActor(ILogger logger)
			: base(logger)
		{
			Receive<HelloWorldRepliedTo>(@event => Execute(Handle, @event));
		}
	}
}