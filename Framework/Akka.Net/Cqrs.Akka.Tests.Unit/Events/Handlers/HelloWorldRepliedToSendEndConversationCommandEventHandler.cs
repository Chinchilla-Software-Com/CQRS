using System;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Commands;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	public class HelloWorldRepliedToSendEndConversationCommandEventHandler
		: IEventHandler<Guid, HelloWorldRepliedTo>
	{
		/// <summary>
		/// Instantiates the <see cref="HelloWorldRepliedToSendEndConversationCommandEventHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public HelloWorldRepliedToSendEndConversationCommandEventHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		public void Handle(HelloWorldRepliedTo message)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorldRepliedToSendEndConversationCommandEventHandlerActor>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion
	}

	public class HelloWorldRepliedToSendEndConversationCommandEventHandlerActor
		: AkkaEventHandler
	{
		protected ICommandSender<Guid> CommandBus { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		public void Handle(HelloWorldRepliedTo message)
		{
			CommandBus.Send(new EndConversationCommand { Id = message.Id });
			UnitTest1.Step3Reached = true;
		}

		#endregion

		public HelloWorldRepliedToSendEndConversationCommandEventHandlerActor(ILogger logger, IAkkaCommandSender<Guid> commandBus)
			: base(logger)
		{
			CommandBus = commandBus;
			Receive<HelloWorldRepliedTo>(@event => Execute(Handle, @event));
		}
	}
}