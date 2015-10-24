#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusReceiver<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandHandlerRegistrar, ICommandReceiver<TAuthenticationToken>
	{
		private static RouteManager Routes { get; set; }

		static AzureCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, false)
		{
		}

		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType)
			where TMessage : IMessage
		{
			Routes.RegisterHandler(handler, targetedType);
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
			switch (command.Framework)
			{
				case FrameworkType.Akka:
					Logger.LogInfo(string.Format("A command arrived of the type '{0}' but was marked as coming from the '{1}' framework, so it was dropped.", command.GetType().FullName, command.Framework));
					return;
			}

			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			Action<IMessage> handler = Routes.GetSingleHandler(command).Delegate;
			handler(command);
		}

		#region Implementation of ICommandReceiver

		public void Start()
		{
			InstantiateReceiving();

			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			ServiceBusReceiver.OnMessage(ReceiveCommand, options);
		}

		#endregion
	}
}