#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Authentication;
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
		public AkkaEventBusProxy(IDependencyResolver dependencyResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			EventHandlerResolver = ((IAkkaAggregateResolver)dependencyResolver).ResolveActor<BusActor>();
		}

		protected IActorRef EventHandlerResolver { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			// We only set these two properties as they are not going to be available across the thread/task
			if (@event.AuthenticationToken == null || @event.AuthenticationToken.Equals(default(TAuthenticationToken)))
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			bool result = EventHandlerResolver.Ask<bool>(@event).Result;
		}

		#endregion

		public class BusActor
			: ReceiveActor
		{
			public BusActor(IAkkaEventPublisher<TAuthenticationToken> eventHandlerResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			{
				EventHandlerResolver = eventHandlerResolver;
				CorrelationIdHelper = correlationIdHelper;
				AuthenticationTokenHelper = authenticationTokenHelper;
				Receive<IEvent<TAuthenticationToken>>(@event => ExecuteReceive(@event));
			}

			protected IAkkaEventPublisher<TAuthenticationToken> EventHandlerResolver { get; private set; }

			protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

			protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

			protected virtual void ExecuteReceive(IEvent<TAuthenticationToken> @event)
			{
				try
				{
					AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);
					CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
					EventHandlerResolver.Publish(@event);

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