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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusReceiver<TAuthenticationToken>
		: AzureEventBus<TAuthenticationToken>
		, IEventHandlerRegistrar
		, IEventReceiver<TAuthenticationToken>
	{
		protected static RouteManager Routes { get; private set; }

		static AzureEventBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureEventBusReceiver(IConfigurationManager configurationManager, IBusHelper busHelper, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
			: base(configurationManager, busHelper, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, false)
		{
		}

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
			RegisterReceiverMessageHandler(ReceiveEvent, options);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
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
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = false)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		protected virtual void ReceiveEvent(BrokeredMessage message)
		{
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				Logger.LogDebug(string.Format("An event message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();
				IEvent<TAuthenticationToken> @event;

				try
				{
					@event = MessageSerialiser.DeserialiseEvent(messageBody);
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
									Logger.LogDebug(string.Format("An event message arrived with the id '{0}' but processing was skipped due to event settings.", message.MessageId));
									return;
								}
							}
						}
					}
					throw;
				}

				CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
				Logger.LogInfo(string.Format("An event message arrived with the id '{0}' was of type {1}.", message.MessageId, @event.GetType().FullName));

				bool canRefresh;
				if (!ConfigurationManager.TryGetSetting(string.Format("{0}.ShouldRefresh", @event.GetType().FullName), out canRefresh))
					canRefresh = false;

				if (canRefresh)
				{
					Task.Factory.StartNew(() =>
					{
						while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
						{
							//Based on LockedUntilUtc property to determine if the lock expires soon
							if (DateTime.UtcNow > message.LockedUntilUtc.AddSeconds(-10))
							{
								// If so, repeat the message
								message.RenewLock();
							}

							Thread.Sleep(500);
						}
					}, brokeredMessageRenewCancellationTokenSource.Token);
				}

				ReceiveEvent(@event);

				// Remove message from queue
				message.Complete();
				Logger.LogDebug(string.Format("An event message arrived and was processed with the id '{0}'.", message.MessageId));

				IList<IEvent<TAuthenticationToken>> events;
				if (EventWaits.TryGetValue(@event.CorrelationId, out events))
					events.Add(@event);
			}
			catch (Exception exception)
			{
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
			}
			finally
			{
				// Cancel the lock of renewing the task
				brokeredMessageRenewCancellationTokenSource.Cancel();
			}
		}

		public virtual void ReceiveEvent(IEvent<TAuthenticationToken> @event)
		{
			switch (@event.Framework)
			{
				case FrameworkType.Akka:
					Logger.LogInfo(string.Format("An event arrived of the type '{0}' but was marked as coming from the '{1}' framework, so it was dropped.", @event.GetType().FullName, @event.Framework));
					return;
			}

			CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);

			Type eventType = @event.GetType();
			bool isRequired = BusHelper.IsEventRequired(eventType);

			IEnumerable<Action<IMessage>> handlers = Routes.GetHandlers(@event, isRequired).Select(x => x.Delegate).ToList();
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (!handlers.Any())
				Logger.LogDebug(string.Format("The event handler for '{0}' is not required.", eventType.FullName));

			foreach (Action<IMessage> handler in handlers)
				handler(@event);
		}
	}
}