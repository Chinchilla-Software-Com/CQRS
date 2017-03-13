#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;
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
			frameworks.Add("Azure-EventHub");
			command.Frameworks = frameworks;
		}

		public virtual bool PrepareAndValidateCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type commandType = command.GetType();

			if (command.Frameworks != null && command.Frameworks.Contains("Azure-EventHub"))
			{
				Logger.LogInfo("The provided command has already been processed in Azure.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, commandType.FullName));
				return false;
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

		public virtual ICommand<TAuthenticationToken> ReceiveCommand(string messageBody, Action<ICommand<TAuthenticationToken>> receiveCommandHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null)
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

			receiveCommandHandler(command);

			return command;
		}

		public virtual void DefaultReceiveCommand(ICommand<TAuthenticationToken> command, RouteManager routeManager, string framework)
		{
			Type commandType = command.GetType();

			if (command.Frameworks != null && command.Frameworks.Contains(framework))
			{
				Logger.LogInfo("The provided command has already been processed in Azure.", string.Format("{0}\\DefaultReceiveCommand({1})", GetType().FullName, commandType.FullName));
				return;
			}

			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			bool isRequired = BusHelper.IsEventRequired(commandType);

			RouteHandlerDelegate commandHandler = routeManager.GetSingleHandler(command, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (commandHandler == null)
			{
				Logger.LogDebug(string.Format("The command handler for '{0}' is not required.", commandType.FullName));
				return;
			}

			Action<IMessage> handler = commandHandler.Delegate;
			handler(command);
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
			frameworks.Add("Azure-EventHub");
			@event.Frameworks = frameworks;
		}

		public virtual bool PrepareAndValidateEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>
		{
			Type eventType = @event.GetType();

			if (@event.Frameworks != null && @event.Frameworks.Contains("Azure-EventHub"))
			{
				Logger.LogInfo("The provided event has already been processed in Azure.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, eventType.FullName));
				return false;
			}

			PrepareEvent(@event, framework);
			return true;
		}

		public virtual IEvent<TAuthenticationToken> ReceiveEvent(string messageBody, Action<IEvent<TAuthenticationToken>> receiveEventHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null)
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

			receiveEventHandler(@event);

			return @event;
		}


		public virtual void DefaultReceiveEvent(IEvent<TAuthenticationToken> @event, RouteManager routeManager, string framework)
		{
			Type eventType = @event.GetType();

			if (@event.Frameworks != null && @event.Frameworks.Contains(framework))
			{
				Logger.LogInfo("The provided event has already been processed in Azure.", string.Format("{0}\\DefaultReceiveEvent({1})", GetType().FullName, eventType.FullName));
				return;
			}

			CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);

			bool isRequired = BusHelper.IsEventRequired(eventType);

			IEnumerable<Action<IMessage>> handlers = routeManager.GetHandlers(@event, isRequired).Select(x => x.Delegate).ToList();
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (!handlers.Any())
				Logger.LogDebug(string.Format("The event handler for '{0}' is not required.", eventType.FullName));

			foreach (Action<IMessage> handler in handlers)
				handler(@event);
		}

		public virtual void RegisterHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			Action<TMessage> registerableEventHandler = message =>
			{
				string telemetryName = message.GetType().Name;
				var telemeteredMessage = message as ITelemeteredMessage;
				if (telemeteredMessage != null)
					telemetryName = telemeteredMessage.TelemetryName;

				if (message is IEvent<TAuthenticationToken>)
					telemetryName = string.Format("Event/{0}", telemetryName);
				else if (message is ICommand<TAuthenticationToken>)
					telemetryName = string.Format("Command/{0}", telemetryName);

				Stopwatch mainStopWatch = Stopwatch.StartNew();
				string responseCode = "200";
				bool wasSuccessfull = true;

				try
				{
					handler(message);
				}
				catch (Exception exception)
				{
					telemetryHelper.TrackException(exception);
					wasSuccessfull = false;
					responseCode = "500";
					throw;
				}
				finally
				{
					mainStopWatch.Stop();
					telemetryHelper.TrackRequest
					(
						string.Format("Cqrs/Handle/{0}", telemetryName),
						DateTimeOffset.UtcNow,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull
					);
				}
			};

			Action<TMessage> registerableHandler = registerableEventHandler;
			if (!holdMessageLock)
			{
				registerableHandler = message =>
				{
					Task.Factory.StartNewSafely(() =>
					{
						registerableEventHandler(message);
					});
				};
			}

			routeManger.RegisterHandler(registerableHandler, targetedType);

			telemetryHelper.TrackEvent(string.Format("Cqrs/RegisterHandler/{0}", typeof(TMessage).Name));
		}
	}
}