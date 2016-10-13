#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBus<TAuthenticationToken>
		, ICommandHandlerRegistrar
		, ICommandReceiver<TAuthenticationToken>
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

		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			Action<TMessage> registerableHandler = handler;
			if (!holdMessageLock)
			{
				registerableHandler = message =>
				{
					Action wrappedEventHandler = () =>
					{
						CorrelationIdHelper.SetCorrelationId(message.CorrelationId);
						handler(message);
					};
					new Task(wrappedEventHandler).Start();
				};
			}

			Routes.RegisterHandler(registerableHandler, targetedType);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		protected virtual void ReceiveCommand(BrokeredMessage message)
		{
			try
			{
				Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();
				ICommand<TAuthenticationToken> command;
				try
				{
					command = MessageSerialiser.DeserialiseCommand(messageBody);
				}
				catch (JsonSerializationException exception)
				{
					JsonSerializationException checkException = exception;
					bool safeToExit = false;
					do
					{
						if (checkException.Message.StartsWith("Could not load assembly"))
						{
							safeToExit = true;
							break;
						}
					} while ((checkException = checkException.InnerException as JsonSerializationException) != null);
					if (safeToExit)
					{
						const string pattern = @"(?<=^Error resolving type specified in JSON ').+?(?='\. Path '\$type')";
						Match match = new Regex(pattern).Match(exception.Message);
						if (match.Success)
						{
							string[] typeParts = match.Value.Split(',');
							if (typeParts.Length == 2)
							{
								string classType = typeParts[0];
								bool isRequired = BusHelper.IsEventRequired(classType);

								if (!isRequired)
								{
									// Remove message from queue
									message.Complete();
									Logger.LogDebug(string.Format("A command message arrived with the id '{0}' but processing was skipped due to command settings.", message.MessageId));
									return;
								}
							}
						}
					}
					throw;
				}

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
			Type commandType = command.GetType();
			switch (command.Framework)
			{
				case FrameworkType.Akka:
					Logger.LogInfo(string.Format("A command arrived of the type '{0}' but was marked as coming from the '{1}' framework, so it was dropped.", commandType.FullName, command.Framework));
					return;
			}

			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			bool isRequired = BusHelper.IsEventRequired(commandType);

			RouteHandlerDelegate commandHandler = Routes.GetSingleHandler(command, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (commandHandler == null)
			{
				Logger.LogDebug(string.Format("The command handler for '{0}' is not required.", commandType.FullName));
				return;
			}

			Action<IMessage> handler = commandHandler.Delegate;
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