#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Chinchilla.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Authentication;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	/// <summary>
	/// Handles the <see cref="HelloWorldRepliedTo"/>.
	/// </summary>
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

		/// <summary>
		/// Resolves Akka.Net actor based <see cref="IAggregateRoot{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		/// <summary>
		/// Responds to the provided <paramref name="message"/> passing the <paramref name="message"/> to an Akka.Net actor.
		/// </summary>
		/// <param name="message">The <see cref="HelloWorldRepliedTo"/> to respond to or "handle"</param>
		public void Handle(HelloWorldRepliedTo message)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorldRepliedToEventHandlerActor>();
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		#endregion
	}

	/// <summary>
	/// An Akka.Net based <see cref="IEventHandler"/> that handles the <see cref="HelloWorldRepliedTo"/>.
	/// </summary>
	public class HelloWorldRepliedToEventHandlerActor
		: AkkaEventHandler<Guid>
	{
		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="HelloWorldRepliedTo"/> to respond to or "handle"</param>
		public void Handle(HelloWorldRepliedTo message)
		{
			AkkaUnitTests.Step2Reached[message.CorrelationId] = true;
		}

		#endregion

		/// <summary>
		/// Instantiates a new instance of <see cref="HelloWorldRepliedToEventHandlerActor"/>.
		/// </summary>
		public HelloWorldRepliedToEventHandlerActor(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<Guid> authenticationTokenHelper)
			: base(logger, correlationIdHelper, authenticationTokenHelper)
		{
			Receive<HelloWorldRepliedTo>(@event => Execute(Handle, @event));
		}
	}
}