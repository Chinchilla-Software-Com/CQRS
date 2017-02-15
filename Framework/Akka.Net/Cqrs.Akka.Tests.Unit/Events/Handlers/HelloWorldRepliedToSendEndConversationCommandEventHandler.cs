using System;
using cdmdotnet.Logging;
using Cqrs.Akka.Commands;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Authentication;

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

		public void Handle(HelloWorldRepliedTo @event)
		{
			global::Akka.Actor.IActorRef item = AggregateResolver.ResolveActor<Actor>();
			// bool result = global::Akka.Actor.Futures.Ask<bool>(item, @event).Result;
			global::Akka.Actor.ActorRefImplicitSenderExtensions.Tell(item, @event);
		}

		#endregion

		public partial class Actor
			: AkkaEventHandler<Guid>
		{
			protected ICommandSender<Guid> CommandBus { get; private set; }

			#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

			public void Handle(HelloWorldRepliedTo message)
			{
				CommandBus.Send(new EndConversationCommand { Id = message.Id });
				AkkaUnitTests.Step3Reached[message.CorrelationId] = true;
			}

			#endregion

			public Actor(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IAkkaCommandSender<Guid> commandBus)
				: base(logger, correlationIdHelper, authenticationTokenHelper)
			{
				CommandBus = commandBus;
				Receive<HelloWorldRepliedTo>(@event => Execute(Handle, @event));
			}
		}
	}
}