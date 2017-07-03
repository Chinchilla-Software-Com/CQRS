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
	public class AzureBusHelper<TAuthenticationToken> : IAzureBusHelper<TAuthenticationToken>
	{
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

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		protected IMessageSerialiser<TAuthenticationToken> MessageSerialiser { get; private set; }

		protected IBusHelper BusHelper { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

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

		public virtual void RefreshLock(CancellationTokenSource brokeredMessageRenewCancellationTokenSource, BrokeredMessage message, string type = "message")
		{
			Task.Factory.StartNewSafely(() =>
			{
				// The capturing of ObjectDisposedException is because even the properties can throw it.
				try
				{
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
									message.RenewLock();
									try
									{
										Logger.LogDebug(string.Format("Renewed the lock on {1} '{0}'.", message.MessageId, type));
									}
									catch
									{
										Trace.TraceError("Renewed the lock on {1} '{0}'.", message.MessageId, type);
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
										Logger.LogWarning(string.Format("Renewing the lock on {1} '{0}' failed as the message lock was lost.", message.MessageId, type), exception: exception);
									}
									catch
									{
										Trace.TraceError("Renewing the lock on {1} '{0}' failed as the message lock was lost.\r\n{2}", message.MessageId, type, exception.Message);
									}
									return;
								}
								catch (Exception exception)
								{
									try
									{
										Logger.LogWarning(string.Format("Renewing the lock on {1} '{0}' failed.", message.MessageId, type), exception: exception);
									}
									catch
									{
										Trace.TraceError("Renewing the lock on {1} '{0}' failed.\r\n{2}", message.MessageId, type, exception.Message);
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

		public virtual void RegisterHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			Action<TMessage> registerableHandler = BusHelper.BuildActionHandler(handler, holdMessageLock);

			routeManger.RegisterHandler(registerableHandler, targetedType);

			telemetryHelper.TrackEvent(string.Format("Cqrs/RegisterHandler/{0}", typeof(TMessage).FullName), new Dictionary<string, string> { { "Type", "Azure/Bus" } });
			telemetryHelper.Flush();
		}
	}
}