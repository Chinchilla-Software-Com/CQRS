#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Chinchilla.StateManagement;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// A helper for command and event buses that also caches <see cref="IConfigurationManager"/> look ups.
	/// </summary>
	public class BusHelper : IBusHelper
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="BusHelper"/>
		/// </summary>
		public BusHelper(IConfigurationManager configurationManager, IContextItemCollectionFactory factory)
		{
			Cache = factory.GetCurrentContext();
			ConfigurationManager = configurationManager;
			CachedChecks = new ConcurrentDictionary<string, Tuple<bool, DateTime>>();
			NullableCachedChecks = new ConcurrentDictionary<string, Tuple<bool?, DateTime>>();
			bool isblackListRequired;
			if (!ConfigurationManager.TryGetSetting("Cqrs.MessageBus.BlackListProcessing", out isblackListRequired))
				isblackListRequired = true;
			EventBlackListProcessing = isblackListRequired;
			StartRefreshCachedChecks();
		}

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// A collection of <see cref="Tuple{T1, T2}"/> holding the configurations value (always a <see cref="bool"/>) and the <see cref="DateTime"/>
		/// The value was last checked, keyed by it's configuration key.
		/// </summary>
		protected IDictionary<string, Tuple<bool, DateTime>> CachedChecks { get; private set; }

		/// <summary>
		/// A collection of <see cref="Tuple{T1, T2}"/> holding the configurations value (always a <see cref="bool"/>) and the <see cref="DateTime"/>
		/// The value was last checked, keyed by it's configuration key.
		/// </summary>
		protected IDictionary<string, Tuple<bool?, DateTime>> NullableCachedChecks { get; private set; }

		/// <summary>
		/// The current value of "Cqrs.MessageBus.BlackListProcessing" from <see cref="ConfigurationManager"/>.
		/// </summary>
		protected bool EventBlackListProcessing { get; private set; }

		/// <summary>
		/// Refreshes <see cref="EventBlackListProcessing"/> and every item currently in <see cref="CachedChecks"/>.
		/// </summary>
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

			// Now in a dictionary safe way check each key for a value.
			keys = NullableCachedChecks.Keys.ToList();
			foreach (string configurationKey in keys)
			{
				Tuple<bool?, DateTime> pair = NullableCachedChecks[configurationKey];
				// Check it's age - by adding 20 minutes from being obtained or refreshed and if it's older than now remove it
				if (pair.Item2.AddMinutes(20) < DateTime.UtcNow)
					NullableCachedChecks.Remove(configurationKey);
			}
		}

		/// <summary>
		/// Starts <see cref="RefreshCachedChecks"/> in a <see cref="Task"/> on a one second loop.
		/// </summary>
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
				// If we can't find a value or there is no specific setting, we default to EventBlackListProcessing
				if (!ConfigurationManager.TryGetSetting(configurationKey, out isRequired))
					isRequired = EventBlackListProcessing;

				// Now cache the response
				try
				{
					CachedChecks.Add(configurationKey, new Tuple<bool, DateTime>(isRequired, DateTime.UtcNow));
				}
				catch (ArgumentException exception)
				{
					if (exception.Message != "The key already existed in the dictionary.")
						throw;
					// It's been added since we checked... adding locks is slow, so just move on.
				}
			}
			// Don't refresh the expiry, we'll just update the cache every so often which is faster than constantly changing dictionary values.
			else
				isRequired = settings.Item1;

			return isRequired;
		}

		/// <summary>
		/// Checks if the private bus is required to send the message. Note, this does not imply the public bus is not required as well.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		/// <returns>Null for unconfigured, True for private bus transmission, false otherwise.</returns>
		public virtual bool? IsPrivateBusRequired(Type messageType)
		{
			return IsABusRequired(messageType, false);
		}

		/// <summary>
		/// Checks if the public bus is required to send the message. Note, this does not imply the public bus is not required as well.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		/// <returns>Null for unconfigured, True for private bus transmission, false otherwise.</returns>
		public virtual bool? IsPublicBusRequired(Type messageType)
		{
			return IsABusRequired(messageType, true);
		}

		/// <summary>
		/// Checks if the particular bus is required to send the message. Note, this does not imply the public bus is not required as well.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		/// <param name="checkPublic">Check for the public or private bus.</param>
		/// <returns>Null for unconfigured, True for a particular bus transmission, false otherwise.</returns>
		protected virtual bool? IsABusRequired(Type messageType, bool checkPublic)
		{
			string configurationKey = string.Format(checkPublic ? "{0}.IsPublicBusRequired" : "{0}.IsPrivateBusRequired", messageType.FullName);
			Tuple<bool?, DateTime> settings;
			bool? isRequired;
			if (!NullableCachedChecks.TryGetValue(configurationKey, out settings))
			{
				bool isRequired1;
				// Check if there is a cached value
				if (ConfigurationManager.TryGetSetting(configurationKey, out isRequired1))
					isRequired = isRequired1;
				// If not, check the attributes
				else if (checkPublic)
				{
					var eventAttribute = Attribute.GetCustomAttribute(messageType, typeof(PublicEventAttribute)) as PublicEventAttribute;
					isRequired = eventAttribute == null ? (bool?) null : true;
				}
				// If not, check the attributes
				else
				{
					var eventAttribute = Attribute.GetCustomAttribute(messageType, typeof(PrivateEventAttribute)) as PrivateEventAttribute;
					isRequired = eventAttribute == null ? (bool?)null : true;
				}

				// Now cache the response
				try
				{
					NullableCachedChecks.Add(configurationKey, new Tuple<bool?, DateTime>(isRequired, DateTime.UtcNow));
				}
				catch (ArgumentException exception)
				{
					if (exception.Message != "The key already existed in the dictionary.")
						throw;
					// It's been added since we checked... adding locks is slow, so just move on.
				}
			}
			// Don't refresh the expiry, we'll just update the cache every so often which is faster than constantly changing dictionary values.
			else
				isRequired = settings.Item1;

			// If all the above is still not difinitive, react to the bus the originating message was received on, but we only need to check for private.
			// We do this here so caching is atleast used, but this cannot be cached as that would be wrong
			if (isRequired == null && !checkPublic)
				if (GetWasPrivateBusUsed())
					return true;

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
				object authenticationToken = null;
				var @event = message as IEvent<TAuthenticationToken>;
				if (@event != null)
				{
					messagePrefix = "Event/";
					telemetryName = string.Format("{0}/{1}/{2}", telemetryName, @event.GetIdentity(), @event.Id);
					authenticationToken = @event.AuthenticationToken;
				}
				else
				{
					var command = message as ICommand<TAuthenticationToken>;
					if (command != null)
					{
						messagePrefix = "Command/";
						telemetryName = string.Format("{0}/{1}/{2}", telemetryName, command.GetIdentity(), command.Id);
						authenticationToken = command.AuthenticationToken;
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
					if (authenticationToken is ISingleSignOnToken)
						telemetryHelper.TrackRequest
						(
							string.Format("Cqrs/Handle/{0}{1}", messagePrefix, telemetryName),
							(ISingleSignOnToken)authenticationToken,
							startedAt,
							mainStopWatch.Elapsed,
							responseCode,
							wasSuccessfull,
							new Dictionary<string, string> { { "Type", source } }
						);
					else if (authenticationToken is Guid)
						telemetryHelper.TrackRequest
						(
							string.Format("Cqrs/Handle/{0}{1}", messagePrefix, telemetryName),
							(Guid?)authenticationToken,
							startedAt,
							mainStopWatch.Elapsed,
							responseCode,
							wasSuccessfull,
							new Dictionary<string, string> { { "Type", source } }
						);
					else if (authenticationToken is int)
						telemetryHelper.TrackRequest
						(
							string.Format("Cqrs/Handle/{0}{1}", messagePrefix, telemetryName),
							(int?)authenticationToken,
							startedAt,
							mainStopWatch.Elapsed,
							responseCode,
							wasSuccessfull,
							new Dictionary<string, string> { { "Type", source } }
						);
					else
					{
						string token = authenticationToken == null ? null : authenticationToken.ToString();
						telemetryHelper.TrackRequest
						(
							string.Format("Cqrs/Handle/{0}{1}", messagePrefix, telemetryName),
							token,
							startedAt,
							mainStopWatch.Elapsed,
							responseCode,
							wasSuccessfull,
							new Dictionary<string, string> { { "Type", source } }
						);
					}

					telemetryHelper.Flush();
				}
			};

			return BuildActionHandler(registerableMessageHandler, holdMessageLock);
		}

		/// <summary>
		/// Build a message handler that implements telemetry capturing as well as off thread handling.
		/// </summary>
		public virtual Action<TMessage> BuildActionHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock)
			where TMessage : IMessage
		{
			Action<TMessage> registerableMessageHandler = handler;

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

		/// <summary>
		/// The key used to store the authentication token in the <see cref="Cache"/>.
		/// </summary>
		protected string CacheKey = "WasPrivateBusUsed";

		/// <summary>
		/// Get or set the Cache.
		/// </summary>
		protected IContextItemCollection Cache { get; private set; }

		/// <summary>
		/// Indicates if the message was received via the private bus or not. If false, this implies the public was use used.
		/// </summary>
		public bool GetWasPrivateBusUsed()
		{
			try
			{
				return Cache.GetData<bool>(CacheKey);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Set whether the message was received via the private bus or not. If false, this indicates the public was use used.
		/// </summary>
		public bool SetWasPrivateBusUsed(bool wasPrivate)
		{
			return Cache.SetData(CacheKey, wasPrivate);
		}
	}
}