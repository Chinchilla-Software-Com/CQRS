#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Akka.Actor;
using Chinchilla.Logging;
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
		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaEventBusProxy{TAuthenticationToken}"/>.
		/// </summary>
		public AkkaEventBusProxy(IDependencyResolver dependencyResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			EventHandlerResolver = ((IAkkaAggregateResolver)dependencyResolver).ResolveActor<BusActor>();
		}

		/// <summary>
		/// Gets the <see cref="IActorRef">event handler resolver</see> that we send/proxy the event to.
		/// </summary>
		protected IActorRef EventHandlerResolver { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}">Authentication Token Helper</see>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		#region Implementation of IEventPublisher<TAuthenticationToken>

		/// <summary>
		/// Publishes the provided <paramref name="event"/> on the event bus.
		/// </summary>
		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			// We only set these two properties as they are not going to be available across the thread/task
			if (@event.AuthenticationToken == null || @event.AuthenticationToken.Equals(default(TAuthenticationToken)))
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			bool result = EventHandlerResolver.Ask<bool>(@event).Result;
		}

		/// <summary>
		/// Publishes the provided <paramref name="events"/> on the event bus.
		/// </summary>
		public virtual void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>
		{
			foreach (TEvent @event in events)
				Publish(@event);
		}

		#endregion

		/// <summary>
		/// Similar to a <see cref="IEventPublisher{TAuthenticationToken}"/>, passes events onto the <see cref="EventHandlerResolver"/>.
		/// </summary>
		public class BusActor
			: ReceiveActor
		{
			/// <summary>
			/// Instantiates a new instance of <see cref="BusActor"/>.
			/// </summary>
			public BusActor(IAkkaEventPublisher<TAuthenticationToken> eventHandlerResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			{
				EventHandlerResolver = eventHandlerResolver;
				CorrelationIdHelper = correlationIdHelper;
				AuthenticationTokenHelper = authenticationTokenHelper;
				Receive<IEvent<TAuthenticationToken>>(@event => ExecuteReceive(@event));
			}

			/// <summary>
			/// Gets or sets the <see cref="IAkkaEventPublisher{TAuthenticationToken}"/>.
			/// </summary>
			protected IAkkaEventPublisher<TAuthenticationToken> EventHandlerResolver { get; private set; }

			/// <summary>
			/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
			/// </summary>
			protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

			/// <summary>
			/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
			/// </summary>
			protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

			/// <summary>
			/// Passes the provided <paramref name="event"/> to <see cref="EventHandlerResolver"/> via <see cref="IEventPublisher{TAuthenticationToken}.Publish{TEvent}(TEvent)"/>
			/// then calls <see cref="ActorRefImplicitSenderExtensions.Tell"/>.
			/// </summary>
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