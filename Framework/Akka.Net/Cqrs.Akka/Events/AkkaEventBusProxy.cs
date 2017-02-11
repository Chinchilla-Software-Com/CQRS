#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Akka.Events
{
	/// <summary>
	/// A <see cref="IEventPublisher{TAuthenticationToken}"/> that proxies <see cref="IEvent{TAuthenticationToken}"/> to the <see cref="IActorRef"/> which acts as a single point of all handler resolutions.
	/// </summary>
	public class AkkaEventBusProxy<TAuthenticationToken>
		: IAkkaEventPublisherProxy<TAuthenticationToken>
	{
		public AkkaEventBusProxy(IDependencyResolver dependencyResolver)
		{
			EventHandlerResolver = (IActorRef)dependencyResolver.Resolve(typeof(BusActor));
		}

		protected IActorRef EventHandlerResolver { get; private set; }

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			bool result = EventHandlerResolver.Ask<bool>(@event).Result;
		}

		#endregion

		public class BusActor
			: ReceiveActor
		{
			public BusActor(IAkkaEventPublisher<TAuthenticationToken> eventHandlerResolver)
			{
				EventHandlerResolver = eventHandlerResolver;
				Receive<IEvent<TAuthenticationToken>>(@event => ExecuteReceive(@event));
			}

			protected IAkkaEventPublisher<TAuthenticationToken> EventHandlerResolver { get; private set; }

			protected virtual void ExecuteReceive(IEvent<TAuthenticationToken> command)
			{
				try
				{
					EventHandlerResolver.Publish(command);

					Sender.Tell(true);
				}
				catch
				{
					Sender.Tell(false);
					throw;
				}
			}
		}
	}
}