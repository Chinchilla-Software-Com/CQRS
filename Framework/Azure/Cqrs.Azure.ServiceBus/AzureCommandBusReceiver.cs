using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Infrastructure;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusReceiver<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandHandlerRegistrar
	{
		protected static IDictionary<Type, List<Action<IMessage>>> Routes { get; private set; }

		static AzureCommandBusReceiver()
		{
			Routes = new Dictionary<Type, List<Action<IMessage>>>();
		}

		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper CorrelationIdHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, CorrelationIdHelper)
		{
			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			ServiceBusClient.OnMessage(ReceiveCommand, options);
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

		protected virtual void ReceiveCommand(BrokeredMessage message)
		{
			try
			{
				Console.WriteLine("MessageID: " + message.MessageId);
				string messageBody = message.GetBody<string>();
				ICommand<TAuthenticationToken> command = MessageSerialiser.DeserialiseCommand(messageBody);

				ReceiveCommand(command);

				// Remove message from queue
				message.Complete();
			}
			catch (Exception)
			{
				// Indicates a problem, unlock message in queue
				message.Abandon();
			}
		}

		protected virtual void ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			List<Action<IMessage>> handlers;
			if (Routes.TryGetValue(command.GetType(), out handlers))
			{
				if (handlers.Count != 1)
					throw new InvalidOperationException("Cannot send to more than one handler");
				handlers.Single()(command);
			}
			else
			{
				throw new InvalidOperationException("No handler registered");
			}
		}
	}
}