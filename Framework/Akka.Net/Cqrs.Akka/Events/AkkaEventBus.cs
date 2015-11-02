#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Akka.Events
{
	/// <summary>
	/// An <see cref="IEventPublisher{TAuthenticationToken}"/> that proxies <see cref="IEvent{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="IEvent{TAuthenticationToken}"/> on the public event bus.
	/// </summary>
	public class AkkaEventBus<TAuthenticationToken>
		: ReceiveActor
		, IAkkaEventBus<TAuthenticationToken>
	{
		protected IActorRef ActorReference { get; private set; }

		protected IEventPublisher<TAuthenticationToken> EventPublisher { get; private set; }

		protected IEventReceiver<TAuthenticationToken> EventReceiver { get; private set; }

		public AkkaEventBus(IEventPublisher<TAuthenticationToken> eventPublisher, IEventReceiver<TAuthenticationToken> eventReceiver)
		{
			ActorReference = null;
			EventPublisher = eventPublisher;
			EventReceiver = eventReceiver;
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			// This will trigger the Akka cycle back publishing... It looks weird, but trust it
			EventReceiver.ReceiveEvent(@event);

			@event.Framework = FrameworkType.Akka;
			EventPublisher.Publish(@event);
		}

		#endregion
	}
}