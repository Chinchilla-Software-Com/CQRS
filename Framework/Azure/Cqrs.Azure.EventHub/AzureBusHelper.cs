#region IMPORTANT NOTE
// This is copied almost exactly into the servicebus except for a string difference. Replicate changes there until a refactor is done.
#endregion

#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A helper for Azure Service Bus and Event Hub.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureBusHelper<TAuthenticationToken> : IAzureBusHelper<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureBusHelper{TAuthenticationToken}"/>.
		/// </summary>
		public AzureBusHelper(IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IBusHelper busHelper, IConfigurationManager configurationManager, IDependencyResolver dependencyResolver)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			MessageSerialiser = messageSerialiser;
			BusHelper = busHelper;
			DependencyResolver = dependencyResolver;
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IMessageSerialiser{TAuthenticationToken}"/>.
		/// </summary>
		protected IMessageSerialiser<TAuthenticationToken> MessageSerialiser { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IBusHelper"/>.
		/// </summary>
		protected IBusHelper BusHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IDependencyResolver"/>.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Prepares a <see cref="ICommand{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TCommand">The <see cref="Type"/> of<see cref="ICommand{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="command"/> is being sent from.</param>
		public virtual void PrepareCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (command.AuthenticationToken == null || command.AuthenticationToken.Equals(default(TAuthenticationToken)))
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			if (string.IsNullOrWhiteSpace(command.OriginatingFramework))
				command.OriginatingFramework = framework;
			var frameworks = new List<string>();
			if (command.Frameworks != null)
				frameworks.AddRange(command.Frameworks);
			frameworks.Add(framework);
			command.Frameworks = frameworks;
		}

		/// <summary>
		/// Prepares and validates a <see cref="ICommand{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TCommand">The <see cref="Type"/> of<see cref="ICommand{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="command"/> is being sent from.</param>
		public virtual bool PrepareAndValidateCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type commandType = command.GetType();

			if (command.Frameworks != null && command.Frameworks.Contains(framework))
			{
				// if this is the only framework in the list, then it's fine to handle as it's just pre-stamped, if there is more than one framework, then exit.
				if (command.Frameworks.Count() != 1)
				{
					Logger.LogInfo("The provided command has already been processed in Azure.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, commandType.FullName));
					return false;
				}
			}

			ICommandValidator<TAuthenticationToken, TCommand> commandValidator = null;
			try
			{
				commandValidator = DependencyResolver.Resolve<ICommandValidator<TAuthenticationToken, TCommand>>();
			}
			catch (Exception exception)
			{
				Logger.LogDebug("Locating an ICommandValidator failed.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, commandType.FullName), exception);
			}

			if (commandValidator != null && !commandValidator.IsCommandValid(command))
			{
				Logger.LogInfo("The provided command is not valid.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, commandType.FullName));
				return false;
			}

			PrepareCommand(command, framework);
			return true;
		}

		/// <summary>
		/// Deserialises and processes the <paramref name="messageBody"/> received from the network through the provided <paramref name="receiveCommandHandler"/>.
		/// </summary>
		/// <param name="messageBody">A serialised <see cref="IMessage"/>.</param>
		/// <param name="receiveCommandHandler">The handler method that will process the <see cref="ICommand{TAuthenticationToken}"/>.</param>
		/// <param name="messageId">The network id of the <see cref="IMessage"/>.</param>
		/// <param name="skippedAction">The <see cref="Action"/> to call when the <see cref="ICommand{TAuthenticationToken}"/> is being skipped.</param>
		/// <param name="lockRefreshAction">The <see cref="Action"/> to call to refresh the network lock.</param>
		/// <returns>The <see cref="ICommand{TAuthenticationToken}"/> that was processed.</returns>
		public virtual ICommand<TAuthenticationToken> ReceiveCommand(string messageBody, Func<ICommand<TAuthenticationToken>, bool?> receiveCommandHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null)
		{
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
							bool isRequired = BusHelper.IsEventRequired(string.Format("{0}.IsRequired", classType));

							if (!isRequired)
							{
								if (skippedAction != null)
									skippedAction();
								return null;
							}
						}
					}
				}
				throw;
			}

			string commandTypeName = command.GetType().FullName;
			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);
			Logger.LogInfo(string.Format("A command message arrived with the {0} was of type {1}.", messageId, commandTypeName));

			bool canRefresh;
			if (!ConfigurationManager.TryGetSetting(string.Format("{0}.ShouldRefresh", commandTypeName), out canRefresh))
				canRefresh = false;

			if (canRefresh)
			{
				if (lockRefreshAction == null)
					Logger.LogWarning(string.Format("A command message arrived with the {0} was of type {1} and was destined to support lock renewal, but no action was provided.", messageId, commandTypeName));
				else
					lockRefreshAction();
			}

			// a false response means the action wasn't handled, but didn't throw an error, so we assume, by convention, that this means it was skipped.
			bool? result = receiveCommandHandler(command);
			if (result != null && !result.Value)
				if (skippedAction != null)
					skippedAction();

			return command;
		}

		/// <summary>
		/// The default command handler that
		/// check if the <see cref="ICommand{TAuthenticationToken}"/> has already been processed by this framework,
		/// checks if the <see cref="ICommand{TAuthenticationToken}"/> is required,
		/// finds the handler from the provided <paramref name="routeManager"/>.
		/// </summary>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to process.</param>
		/// <param name="routeManager">The <see cref="RouteManager"/> to get the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> from.</param>
		/// <param name="framework">The current framework.</param>
		/// <returns>
		/// True indicates the <paramref name="command"/> was successfully handled by a handler.
		/// False indicates the <paramref name="command"/> wasn't handled, but didn't throw an error, so by convention, that means it was skipped.
		/// Null indicates the command<paramref name="command"/> wasn't handled as it was already handled.
		/// </returns>
		public virtual bool? DefaultReceiveCommand(ICommand<TAuthenticationToken> command, RouteManager routeManager, string framework)
		{
			Type commandType = command.GetType();

			if (command.Frameworks != null && command.Frameworks.Contains(framework))
			{
				// if this is the only framework in the list, then it's fine to handle as it's just pre-stamped, if there is more than one framework, then exit.
				if (command.Frameworks.Count() != 1)
				{
					Logger.LogInfo("The provided command has already been processed in Azure.", string.Format("{0}\\DefaultReceiveCommand({1})", GetType().FullName, commandType.FullName));
					return null;
				}
			}

			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			bool isRequired = BusHelper.IsEventRequired(commandType);

			RouteHandlerDelegate commandHandler = routeManager.GetSingleHandler(command, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (commandHandler == null)
			{
				Logger.LogDebug(string.Format("The command handler for '{0}' is not required.", commandType.FullName));
				return false;
			}

			Action<IMessage> handler = commandHandler.Delegate;
			handler(command);
			return true;
		}

		/// <summary>
		/// Prepares an <see cref="IEvent{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of<see cref="IEvent{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="event"/> is being sent from.</param>
		public virtual void PrepareEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (@event.AuthenticationToken == null || @event.AuthenticationToken.Equals(default(TAuthenticationToken)))
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();
			@event.TimeStamp = DateTimeOffset.UtcNow;

			if (string.IsNullOrWhiteSpace(@event.OriginatingFramework))
				@event.OriginatingFramework = framework;
			var frameworks = new List<string>();
			if (@event.Frameworks != null)
				frameworks.AddRange(@event.Frameworks);
			frameworks.Add(framework);
			@event.Frameworks = frameworks;
		}

		/// <summary>
		/// Prepares and validates an <see cref="IEvent{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of<see cref="IEvent{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="event"/> is being sent from.</param>
		public virtual bool PrepareAndValidateEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>
		{
			Type eventType = @event.GetType();

			if (@event.Frameworks != null && @event.Frameworks.Contains(framework))
			{
				// if this is the only framework in the list, then it's fine to handle as it's just pre-stamped, if there is more than one framework, then exit.
				if (@event.Frameworks.Count() != 1)
				{
					Logger.LogInfo("The provided event has already been processed in Azure.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, eventType.FullName));
					return false;
				}
			}

			PrepareEvent(@event, framework);
			return true;
		}

		/// <summary>
		/// Deserialises and processes the <paramref name="messageBody"/> received from the network through the provided <paramref name="receiveEventHandler"/>.
		/// </summary>
		/// <param name="messageBody">A serialised <see cref="IMessage"/>.</param>
		/// <param name="receiveEventHandler">The handler method that will process the <see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="messageId">The network id of the <see cref="IMessage"/>.</param>
		/// <param name="skippedAction">The <see cref="Action"/> to call when the <see cref="IEvent{TAuthenticationToken}"/> is being skipped.</param>
		/// <param name="lockRefreshAction">The <see cref="Action"/> to call to refresh the network lock.</param>
		/// <returns>The <see cref="IEvent{TAuthenticationToken}"/> that was processed.</returns>
		public virtual IEvent<TAuthenticationToken> ReceiveEvent(string messageBody, Func<IEvent<TAuthenticationToken>, bool?> receiveEventHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null)
		{
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
							bool isRequired = BusHelper.IsEventRequired(string.Format("{0}.IsRequired", classType));

							if (!isRequired)
							{
								if (skippedAction != null)
									skippedAction();
								return null;
							}
						}
					}
				}
				throw;
			}

			string eventTypeName = @event.GetType().FullName;
			CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);
			Logger.LogInfo(string.Format("An event message arrived with the {0} was of type {1}.", messageId, eventTypeName));

			bool canRefresh;
			if (!ConfigurationManager.TryGetSetting(string.Format("{0}.ShouldRefresh", eventTypeName), out canRefresh))
				canRefresh = false;

			if (canRefresh)
			{
				if (lockRefreshAction == null)
					Logger.LogWarning(string.Format("An event message arrived with the {0} was of type {1} and was destined to support lock renewal, but no action was provided.", messageId, eventTypeName));
				else
					lockRefreshAction();
			}

			// a false response means the action wasn't handled, but didn't throw an error, so we assume, by convention, that this means it was skipped.
			bool? result = receiveEventHandler(@event);
			if (result != null && !result.Value)
				if (skippedAction != null)
					skippedAction();

			return @event;
		}

		/// <summary>
		/// Refreshes the network lock.
		/// </summary>
		public virtual void RefreshLock(CancellationTokenSource brokeredMessageRenewCancellationTokenSource, BrokeredMessage message, string type = "message")
		{
			Task.Factory.StartNewSafely(() =>
			{
				// The capturing of ObjectDisposedException is because even the properties can throw it.
				try
				{
					object value;
					string typeName = null;
					if (message.Properties.TryGetValue("Type", out value))
						typeName = value.ToString();

					long loop = long.MinValue;
					while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
					{
						// Based on LockedUntilUtc property to determine if the lock expires soon
						// We lock for 45 seconds to ensure any thread based issues are mitigated.
						if (DateTime.UtcNow > message.LockedUntilUtc.AddSeconds(-45))
						{
							// If so, renew the lock
							for (int i = 0; i < 10; i++)
							{
								try
								{
									if (brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
										return;
									message.RenewLock();
									try
									{
										Logger.LogDebug(string.Format("Renewed the {2} lock on {1} '{0}'.", message.MessageId, type, typeName));
									}
									catch
									{
										Trace.TraceError("Renewed the {2} lock on {1} '{0}'.", message.MessageId, type, typeName);
									}

									break;
								}
								catch (ObjectDisposedException)
								{
									return;
								}
								catch (MessageLockLostException exception)
								{
									try
									{
										Logger.LogWarning(string.Format("Renewing the {2} lock on {1} '{0}' failed as the message lock was lost.", message.MessageId, type, typeName), exception: exception);
									}
									catch
									{
										Trace.TraceError("Renewing the {2} lock on {1} '{0}' failed as the message lock was lost.\r\n{3}", message.MessageId, type, typeName, exception.Message);
									}
									return;
								}
								catch (Exception exception)
								{
									try
									{
										Logger.LogWarning(string.Format("Renewing the {2} lock on {1} '{0}' failed.", message.MessageId, type, typeName), exception: exception);
									}
									catch
									{
										Trace.TraceError("Renewing the {2} lock on {1} '{0}' failed.\r\n{3}", message.MessageId, type, typeName, exception.Message);
									}
									if (i == 9)
										return;
								}
							}
						}

						if (loop++ % 5 == 0)
							Thread.Yield();
						else
							Thread.Sleep(500);
						if (loop == long.MaxValue)
							loop = long.MinValue;
					}
					try
					{
						brokeredMessageRenewCancellationTokenSource.Dispose();
					}
					catch (ObjectDisposedException) { }
				}
				catch (ObjectDisposedException) { }
			}, brokeredMessageRenewCancellationTokenSource.Token);
		}

		/// <summary>
		/// The default event handler that
		/// check if the <see cref="IEvent{TAuthenticationToken}"/> has already been processed by this framework,
		/// checks if the <see cref="IEvent{TAuthenticationToken}"/> is required,
		/// finds the handler from the provided <paramref name="routeManager"/>.
		/// </summary>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to process.</param>
		/// <param name="routeManager">The <see cref="RouteManager"/> to get the <see cref="IEventHandler{TAuthenticationToken,TCommand}"/> from.</param>
		/// <param name="framework">The current framework.</param>
		/// <returns>
		/// True indicates the <paramref name="event"/> was successfully handled by a handler.
		/// False indicates the <paramref name="event"/> wasn't handled, but didn't throw an error, so by convention, that means it was skipped.
		/// Null indicates the <paramref name="event"/> wasn't handled as it was already handled.
		/// </returns>
		public virtual bool? DefaultReceiveEvent(IEvent<TAuthenticationToken> @event, RouteManager routeManager, string framework)
		{
			Type eventType = @event.GetType();

			if (@event.Frameworks != null && @event.Frameworks.Contains(framework))
			{
				// if this is the only framework in the list, then it's fine to handle as it's just pre-stamped, if there is more than one framework, then exit.
				if (@event.Frameworks.Count() != 1)
				{
					Logger.LogInfo("The provided event has already been processed in Azure.", string.Format("{0}\\DefaultReceiveEvent({1})", GetType().FullName, eventType.FullName));
					return null;
				}
			}

			CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);

			bool isRequired = BusHelper.IsEventRequired(eventType);

			IEnumerable<Action<IMessage>> handlers = routeManager.GetHandlers(@event, isRequired).Select(x => x.Delegate).ToList();
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (!handlers.Any())
			{
				Logger.LogDebug(string.Format("The event handler for '{0}' is not required.", eventType.FullName));
				return false;
			}

			foreach (Action<IMessage> handler in handlers)
				handler(@event);
			return true;
		}

		/// <summary>
		/// Manually registers the provided <paramref name="handler"/> 
		/// on the provided <paramref name="routeManger"/>
		/// </summary>
		/// <typeparam name="TMessage">The <see cref="Type"/> of <see cref="IMessage"/> the <paramref name="handler"/> can handle.</typeparam>
		public virtual void RegisterHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			Action<TMessage> registerableHandler = BusHelper.BuildActionHandler(handler, holdMessageLock);

			routeManger.RegisterHandler(registerableHandler, targetedType);

			telemetryHelper.TrackEvent(string.Format("Cqrs/RegisterHandler/{0}", typeof(TMessage).FullName), new Dictionary<string, string> { { "Type", "Azure/Bus" } });
			telemetryHelper.Flush();
		}

		/// <summary>
		/// Register an event handler that will listen and respond to all events.
		/// </summary>
		public void RegisterGlobalEventHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler,
			bool holdMessageLock = true) where TMessage : IMessage
		{
			Action<TMessage> registerableHandler = BusHelper.BuildActionHandler(handler, holdMessageLock);

			routeManger.RegisterGlobalEventHandler(registerableHandler);

			telemetryHelper.TrackEvent(string.Format("Cqrs/RegisterGlobalEventHandler/{0}", typeof(TMessage).FullName), new Dictionary<string, string> { { "Type", "Azure/Bus" } });
			telemetryHelper.Flush();
		}
	}
}