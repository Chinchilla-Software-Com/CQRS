#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Commands;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Authentication;
using Cqrs.Domain;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	/// <summary>
	/// Handles the <see cref="HelloWorldRepliedTo"/> and sends a <see cref="EndConversationCommand"/>.
	/// </summary>
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

		/// <summary>
		/// Resolves Akka.Net actor based <see cref="IAggregateRoot{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		/// <summary>
		/// Responds to the provided <paramref name="event"/> passing the <paramref name="event"/> to an Akka.Net actor.
		/// </summary>
		/// <param name="event">The <see cref="HelloWorldRepliedTo"/> to respond to or "handle"</param>
		public void Handle(HelloWorldRepliedTo @event)
		{
			IActorRef item = AggregateResolver.ResolveActor<Actor>();
			// bool result = global::Akka.Actor.Futures.Ask<bool>(item, @event).Result;
			ActorRefImplicitSenderExtensions.Tell(item, @event);
		}

		#endregion

		/// <summary>
		/// An Akka.Net based <see cref="IEventHandler"/> that handles the <see cref="HelloWorldRepliedTo"/>.
		/// </summary>
		public class Actor
			: AkkaEventHandler<Guid>
		{
			/// <summary>
			/// Publish any <see cref="ICommand{TAuthenticationToken}"/> instances that you want to send with this.
			/// </summary>
			protected ICommandPublisher<Guid> CommandBus { get; private set; }

			#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

			/// <summary>
			/// Responds to the provided <paramref name="message"/>.
			/// </summary>
			/// <param name="message">The <see cref="HelloWorldRepliedTo"/> to respond to or "handle"</param>
			public void Handle(HelloWorldRepliedTo message)
			{
				CommandBus.Publish(new EndConversationCommand { Id = message.Id });
				AkkaUnitTests.Step3Reached[message.CorrelationId] = true;
			}

			#endregion

			/// <summary>
			/// Instantiates a new instance of <see cref="Actor"/>.
			/// </summary>
			public Actor(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, IAkkaCommandPublisher<Guid> commandBus)
				: base(logger, correlationIdHelper, authenticationTokenHelper)
			{
				CommandBus = commandBus;
				Receive<HelloWorldRepliedTo>(@event => Execute(Handle, @event));
			}
		}
	}
}