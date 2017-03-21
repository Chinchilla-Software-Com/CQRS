using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
			CachedChecks = new ConcurrentDictionary<string, Tuple<bool, DateTime>>();
			bool isblackListRequired;
			if (!ConfigurationManager.TryGetSetting("Cqrs.MessageBus.BlackListProcessing", out isblackListRequired))
				isblackListRequired = true;
			EventBlackListProcessing = isblackListRequired;
			StartRefreshCachedChecks();
		}

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected IDictionary<string, Tuple<bool, DateTime>> CachedChecks { get; private set; }

		protected bool EventBlackListProcessing { get; private set; }

		protected virtual void RefreshCachedChecks()
		{
			// First refresh the EventBlackListProcessing property
			bool isblackListRequired;
			if (!ConfigurationManager.TryGetSetting("Cqrs.MessageBus.BlackListProcessing", out isblackListRequired))
				isblackListRequired = true;
			EventBlackListProcessing = isblackListRequired;

			// Now in a dictionary safe way check each key for a value.
			IList<string> keys = CachedChecks.Keys.ToList();
			foreach (string configurationKey in keys)
			{
				Tuple<bool, DateTime> pair = CachedChecks[configurationKey];
				bool value;
				// If we can't a value or there is no specific setting, remove it from the cache
				if (!ConfigurationManager.TryGetSetting(configurationKey, out value))
					CachedChecks.Remove(configurationKey);
				// Refresh the value and reset it's expiry if the value has changed
				else if (pair.Item1 != value)
					CachedChecks[configurationKey] = new Tuple<bool, DateTime>(value, DateTime.UtcNow);
				// Check it's age - by adding 20 minutes from being obtained or refreshed and if it's older than now remove it
				else if (pair.Item2.AddMinutes(20) < DateTime.UtcNow)
					CachedChecks.Remove(configurationKey);
			}
		}

		protected virtual void StartRefreshCachedChecks()
		{
			Task.Factory.StartNewSafely(() =>
			{
				long loop = 0;
				while (true)
				{
					RefreshCachedChecks();

					if (loop++%5 == 0)
						Thread.Yield();
					else
						Thread.Sleep(1000);
					if (loop == long.MaxValue)
						loop = long.MinValue;
				}
			});
		}

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
			Tuple<bool, DateTime> settings;
			bool isRequired;
			if (!CachedChecks.TryGetValue(configurationKey, out settings))
			{
				// If we can't a value or there is no specific setting, we default to EventBlackListProcessing
				if (!ConfigurationManager.TryGetSetting(configurationKey, out isRequired))
					isRequired = EventBlackListProcessing;

				// Now cache the response
				CachedChecks.Add(configurationKey, new Tuple<bool, DateTime>(isRequired, DateTime.UtcNow));
			}
			// Don't refresh the expiry, we'll just update the cache every so often which is faster than constantly changing dictionary values.
			else
				isRequired = settings.Item1;

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