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
	public class AzureMessageBusReceiver<TAuthenticationToken> : AzureMessageBus<TAuthenticationToken>, IHandlerRegistrar
	{
		protected static IDictionary<Type, List<Action<IMessage>>> Routes { get; private set; }

		static AzureMessageBusReceiver()
		{
			Routes = new Dictionary<Type, List<Action<IMessage>>>();
		}

		public AzureMessageBusReceiver(IConfigurationManager configurationManager, IEventSerialiser<TAuthenticationToken> eventSerialiser)
			: base(configurationManager, eventSerialiser)
		{
			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			ServiceBusClient.OnMessage(ReceiveEvents, options);
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

		protected virtual void ReceiveEvents(BrokeredMessage message)
		{
			try
			{
				Console.WriteLine("MessageID: " + message.MessageId);
				IEvent<TAuthenticationToken> @event = EventSerialiser.DeserialisEvent(message.GetBody<string>());

				List<Action<IMessage>> handlers;
				if (!Routes.TryGetValue(@event.GetType(), out handlers))
					return;
				foreach (Action<IMessage> handler in handlers)
					handler(@event);

				// Remove message from queue
				message.Complete();
			}
			catch (Exception)
			{
				// Indicates a problem, unlock message in queue
				message.Abandon();
			}
		}
	}
}