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
	/// An <see cref="IEventPublisher{TAuthenticationToken}"/> that proxies <see cref="IEvent{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="IEvent{TAuthenticationToken}"/> on the public command bus.
	/// </summary>
	public class AkkaEventBus<TAuthenticationToken>
		: IEventPublisher<TAuthenticationToken>
	{
		internal AkkaEventBus(IActorRef actorReference, IEventPublisher<TAuthenticationToken> eventPublisher)
		{
			ActorReference = actorReference;
			EventPublisher = eventPublisher;
		}

		protected IActorRef ActorReference { get; private set; }

		protected IEventPublisher<TAuthenticationToken> EventPublisher { get; private set; }

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			ActorReference.Tell(@event);

			@event.Framework = FrameworkType.Akka;
			EventPublisher.Publish(@event);
		}

		#endregion
	}
}