using System;
using System.Collections.Generic;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Infrastructure;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusReceiver<TAuthenticationToken> : AzureEventBus<TAuthenticationToken>, IEventHandlerRegistrar
	{
		protected static IDictionary<Type, List<Action<IMessage>>> Routes { get; private set; }

		static AzureEventBusReceiver()
		{
			Routes = new Dictionary<Type, List<Action<IMessage>>>();
		}

		public AzureEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, false, true)
		{
			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			ServiceBusReceiver.OnMessage(ReceiveEvent, options);
		}

		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage
		{
			List<Action<IMessage>> handlers;
			if (!Routes.TryGetValue(typeof(TMessage), out handlers))
			{
				handlers = new List<Action<IMessage>>();
				Routes.Add(typeof(TMessage), handlers);
			}
			handlers.Add(DelegateAdjuster.CastArgument<IMessage, TMessage>(x => handler(x)));
		}

		protected virtual void ReceiveEvent(BrokeredMessage message)
		{
			try
			{
				Logger.LogDebug(string.Format("An event message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();
				IEvent<TAuthenticationToken> @event = MessageSerialiser.DeserialiseEvent(messageBody);

				CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
				Logger.LogInfo(string.Format("An event message arrived with the id '{0}' was of type {1}.", message.MessageId, @event.GetType().FullName));

				ReceiveEvent(@event);

				// Remove message from queue
				message.Complete();
				Logger.LogDebug(string.Format("An event message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (Exception exception)
			{
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
			}
		}

		protected virtual void ReceiveEvent(IEvent<TAuthenticationToken> @event)
		{
			CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);

			List<Action<IMessage>> handlers;
			if (!Routes.TryGetValue(@event.GetType(), out handlers))
				return;
			foreach (Action<IMessage> handler in handlers)
				handler(@event);
		}
	}
}