using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public class BusHelper : IBusHelper
	{
		public BusHelper(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Checks if a white-list or black-list approach is taken, then checks the <see cref="IConfigurationManager"/> to see if a key exists defining if the event is required or not.
		/// If the event is required and it cannot be resolved, an error will be raised.
		/// Otherwise the event will be marked as processed.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		public virtual bool IsEventRequired(Type messageType)
		{
			return IsEventRequired(string.Format("{0}.IsRequired", messageType.FullName));
		}

		/// <summary>
		/// Checks if a white-list or black-list approach is taken, then checks the <see cref="IConfigurationManager"/> to see if a key exists defining if the event is required or not.
		/// If the event is required and it cannot be resolved, an error will be raised.
		/// Otherwise the event will be marked as processed.
		/// </summary>
		/// <param name="configurationKey">The configuration key to check.</param>
		public virtual bool IsEventRequired(string configurationKey)
		{
			bool isblackListRequired;
			if (!ConfigurationManager.TryGetSetting("Cqrs.MessageBus.BlackListProcessing", out isblackListRequired))
				isblackListRequired = true;

			bool isRequired;
			if (!ConfigurationManager.TryGetSetting(configurationKey, out isRequired))
				isRequired = isblackListRequired;

			return isRequired;
		}

		/// <summary>
		/// Build a message handler that implements telemetry capturing as well as off thread handling.
		/// </summary>
		public virtual Action<TMessage> BuildTelemeteredActionHandler<TMessage, TAuthenticationToken>(ITelemetryHelper telemetryHelper, Action<TMessage> handler, bool holdMessageLock, string source)
			where TMessage : IMessage
		{
			Action<TMessage> registerableMessageHandler = message =>
			{
				DateTimeOffset startedAt = DateTimeOffset.UtcNow;
				Stopwatch mainStopWatch = Stopwatch.StartNew();
				string responseCode = "200";
				bool wasSuccessfull = true;

				string telemetryName = message.GetType().FullName;
				var telemeteredMessage = message as ITelemeteredMessage;
				string messagePrefix = null;
				var @event = message as IEvent<TAuthenticationToken>;
				if (@event != null)
				{
					messagePrefix = "Event/";
					telemetryName = string.Format("{0}/{1}", telemetryName, @event.Id);
				}
				else
				{
					var command = message as ICommand<TAuthenticationToken>;
					if (command != null)
					{
						messagePrefix = "Command/";
						telemetryName = string.Format("{0}/{1}", telemetryName, command.Id);
					}
				}

				if (telemeteredMessage != null)
					telemetryName = telemeteredMessage.TelemetryName;

				telemetryHelper.TrackEvent(string.Format("Cqrs/Handle/{0}{1}/Started", messagePrefix, telemetryName));

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
					telemetryHelper.TrackEvent(string.Format("Cqrs/Handle/{0}{1}/Finished", messagePrefix, telemetryName));

					mainStopWatch.Stop();
					telemetryHelper.TrackRequest
					(
						string.Format("Cqrs/Handle/{0}{1}", messagePrefix, telemetryName),
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull,
						new Dictionary<string, string> { { "Type", source } }
					);

					telemetryHelper.Flush();
				}
			};

			Action<TMessage> registerableHandler = registerableMessageHandler;
			if (!holdMessageLock)
			{
				registerableHandler = message =>
				{
					Task.Factory.StartNewSafely(() =>
					{
						registerableMessageHandler(message);
					});
				};
			}

			return registerableHandler;
		}
	}
}