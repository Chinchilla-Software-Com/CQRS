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
	public class AzureCommandBusReceiver<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandHandlerRegistrar, ICommandReceiver<TAuthenticationToken>
	{
		protected static IDictionary<Type, List<Action<IMessage>>> Routes { get; private set; }

		static AzureCommandBusReceiver()
		{
			Routes = new Dictionary<Type, List<Action<IMessage>>>();
		}

		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, false, true)
		{
			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			ServiceBusReceiver.OnMessage(ReceiveCommand, options);
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
				Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();
				ICommand<TAuthenticationToken> command = MessageSerialiser.DeserialiseCommand(messageBody);

				CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
				Logger.LogInfo(string.Format("A command message arrived with the id '{0}' was of type {1}.", message.MessageId, command.GetType().FullName));

				ReceiveCommand(command);

				// Remove message from queue
				message.Complete();
				Logger.LogDebug(string.Format("A command message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (Exception exception)
			{
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
			}
		}

		public virtual void ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			List<Action<IMessage>> handlers;
			if (Routes.TryGetValue(command.GetType(), out handlers))
			{
				if (handlers.Count != 1)
					throw new MultipleCommandHandlersRegisteredException(command.GetType());
				handlers.Single()(command);
			}
			else
			{
				throw new NoCommandHandlerRegisteredException(command.GetType());
			}
		}
	}
}