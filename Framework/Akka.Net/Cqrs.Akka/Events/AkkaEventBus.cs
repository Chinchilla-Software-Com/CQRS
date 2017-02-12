#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Akka.Events
{
	/// <summary>
	/// An <see cref="IEventPublisher{TAuthenticationToken}"/> that proxies <see cref="IEvent{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="IEvent{TAuthenticationToken}"/> on the public event bus.
	/// </summary>
	public class AkkaEventBus<TAuthenticationToken>
		: IAkkaEventPublisher<TAuthenticationToken>
		, IEventHandlerRegistrar
	{
		protected static RouteManager Routes { get; private set; }

		static AkkaEventBus()
		{
			Routes = new RouteManager();
		}

		protected IEventPublisher<TAuthenticationToken> EventPublisher { get; private set; }

		protected IEventReceiver<TAuthenticationToken> EventReceiver { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected IBusHelper BusHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		public AkkaEventBus(IBusHelper busHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IEventPublisher<TAuthenticationToken> eventPublisher, IEventReceiver<TAuthenticationToken> eventReceiver)
		{
			BusHelper = busHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			EventPublisher = eventPublisher;
			EventReceiver = eventReceiver;
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			IEnumerable<RouteHandlerDelegate> handlers;
			if (!PrepareAndValidateEvent(@event, "Akka", out handlers))
				return;

			// This could be null if Akka won't handle the command and something else will.
			foreach (RouteHandlerDelegate eventHandler in handlers)
				eventHandler.Delegate(@event);

			// Let everything else know about the event
			EventPublisher.Publish(@event);
		}

		#endregion


		public virtual void PrepareEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (@event.AuthenticationToken == null || @event.AuthenticationToken.Equals(default(TAuthenticationToken)))
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			if (string.IsNullOrWhiteSpace(@event.OriginatingFramework))
			{
				@event.TimeStamp = DateTimeOffset.UtcNow;
				@event.OriginatingFramework = framework;
			}
			var frameworks = new List<string>();
			if (@event.Frameworks != null)
				frameworks.AddRange(@event.Frameworks);
			frameworks.Add(framework);
			@event.Frameworks = frameworks;
		}

		public virtual bool PrepareAndValidateEvent<TEvent>(TEvent @event, string framework, out IEnumerable<RouteHandlerDelegate> handlers)
			where TEvent : IEvent<TAuthenticationToken>
		{
			Type eventType = @event.GetType();

			if (@event.Frameworks != null && @event.Frameworks.Contains(framework))
			{
				Logger.LogInfo("The provided event has already been processed in Akka.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, eventType.FullName));
				handlers = Enumerable.Empty<RouteHandlerDelegate>();
				return false;
			}

			PrepareEvent(@event, framework);


			bool isRequired = BusHelper.IsEventRequired(eventType);

			handlers = Routes.GetHandlers(@event, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (handlers == null || !handlers.Any())
				Logger.LogDebug(string.Format("An event handler for '{0}' is not required.", eventType.FullName));

			return true;
		}


		#region Implementation of IHandlerRegistrar

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		/// <remarks>
		/// In many cases the <paramref name="targetedType"/> will be the event handler class itself, what you actually want is the target of what is being updated
		/// </remarks>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true) where TMessage : IMessage
		{
			Routes.RegisterHandler(handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true) where TMessage : IMessage
		{
			RegisterHandler(handler, null);
		}

		#endregion
	}
}