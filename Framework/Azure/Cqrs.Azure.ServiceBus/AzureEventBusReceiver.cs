using System;
using System.Collections.Generic;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Infrastructure;
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

		public AzureEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser)
			: base(configurationManager, messageSerialiser)
		{
			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			ServiceBusClient.OnMessage(ReceiveEvent, options);
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
				Console.WriteLine("MessageID: " + message.MessageId);
				IEvent<TAuthenticationToken> @event = MessageSerialiser.DeserialiseEvent(message.GetBody<string>());

				ReceiveEvent(@event);

				// Remove message from queue
				message.Complete();
			}
			catch (Exception)
			{
				// Indicates a problem, unlock message in queue
				message.Abandon();
			}
		}

		protected virtual void ReceiveEvent(IEvent<TAuthenticationToken> @event)
		{
			List<Action<IMessage>> handlers;
			if (!Routes.TryGetValue(@event.GetType(), out handlers))
				return;
			foreach (Action<IMessage> handler in handlers)
				handler(@event);
		}
	}
}