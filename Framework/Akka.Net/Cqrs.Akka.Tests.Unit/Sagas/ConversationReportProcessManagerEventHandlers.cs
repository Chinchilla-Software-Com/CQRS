using System;
using Akka.Actor;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Events;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Sagas
{
	/// <summary>
	/// An <see cref="IEventHandler"/> that passes the <see cref="IEvent{TAuthenticationToken}"/> instances it receives to <see cref="ConversationReportProcessManager"/>
	/// </summary>
	public class ConversationReportProcessManagerEventHandlers
		: IEventHandler<Guid, HelloWorldSaid>
			, IEventHandler<Guid, HelloWorldRepliedTo>
			, IEventHandler<Guid, ConversationEnded>
	{
		/// <summary>
		/// Instantiates the <see cref="ConversationReportProcessManagerEventHandlers"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public ConversationReportProcessManagerEventHandlers(IAkkaSagaResolver sagaResolver)
		{
			SagaResolver = sagaResolver;
		}

		/// <summary>
		/// Resolves Akka.Net actor based <see cref="ISaga{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaSagaResolver SagaResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldRepliedTo>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="IEvent{TAuthenticationToken}"/> to respond to or "handle"</param>
		protected virtual void HandleEvent(IEvent<Guid> message)
		{
			// Resolve and locate the instance of the Saga to pass the message to
			IActorRef item = SagaResolver.ResolveActor<ConversationReportProcessManager, Guid>(message.Id);
			// Pass the message to it (and wait?)
			bool result = item.Ask<bool>(message).Result;
			// item.Tell(message);
		}

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="HelloWorldRepliedTo"/> to respond to or "handle"</param>
		public void Handle(HelloWorldRepliedTo message)
		{
			HandleEvent(message);
		}

		#endregion

		#region Implementation of IMessageHandler<in HelloWorldSaid>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="HelloWorldSaid"/> to respond to or "handle"</param>
		public void Handle(HelloWorldSaid message)
		{
			HandleEvent(message);
		}

		#endregion

		#region Implementation of IMessageHandler<in ConversationEnded>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="ConversationEnded"/> to respond to or "handle"</param>
		public void Handle(ConversationEnded message)
		{
			HandleEvent(message);
		}

		#endregion
	}
}