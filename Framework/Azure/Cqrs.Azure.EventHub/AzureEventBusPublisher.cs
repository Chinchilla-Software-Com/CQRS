﻿#region Copyright
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
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;

#if NETSTANDARD2_0
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using EventData = Microsoft.Azure.EventHubs.EventData;
#else
using Microsoft.ServiceBus.Messaging;
using EventData = Microsoft.ServiceBus.Messaging.EventData;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// An <see cref="IEventPublisher{TAuthenticationToken}"/> that resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventBusPublisher<TAuthenticationToken>
		: AzureEventHubBus<TAuthenticationToken>
		, IEventPublisher<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureEventBusPublisher{TAuthenticationToken}"/>.
		/// </summary>
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IHashAlgorithmFactory hashAlgorithmFactory, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, hashAlgorithmFactory, azureBusHelper, true)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventHub.EventBus.Publisher.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		/// <summary>
		/// Publishes the provided <paramref name="event"/> on the event bus.
		/// </summary>
		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (@event == null)
			{
				Logger.LogDebug("No event to publish.");
				return;
			}
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool wasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/EventHub" } };
			string telemetryName = string.Format("{0}/{1}/{2}", @event.GetType().FullName, @event.GetIdentity(), @event.Id);
			var telemeteredEvent = @event as ITelemeteredMessage;
			if (telemeteredEvent != null)
				telemetryName = telemeteredEvent.TelemetryName;
			telemetryName = string.Format("Event/{0}", telemetryName);

			try
			{
				if (!AzureBusHelper.PrepareAndValidateEvent(@event, "Azure-EventHub"))
					return;

				try
				{
					var brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseEvent, @event.GetType(), @event);

#if NETSTANDARD2_0
					EventHubPublisher.SendAsync(brokeredMessage).Wait();
#else
					EventHubPublisher.Send(brokeredMessage);
#endif
					wasSuccessfull = true;
				}
				catch (QuotaExceededException exception)
				{
					responseCode = "429";
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish an event.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
			}
			finally
			{
				TelemetryHelper.TrackDependency("Azure/EventHub/EventBus", "Event", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
			}
			Logger.LogInfo(string.Format("An event was published with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
		}

		/// <summary>
		/// Publishes the provided <paramref name="events"/> on the event bus.
		/// </summary>
		public virtual void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (events == null)
			{
				Logger.LogDebug("No events to publish.");
				return;
			}
			IList<TEvent> sourceEvents = events.ToList();
			if (!sourceEvents.Any())
			{
				Logger.LogDebug("An empty collection of events to publish.");
				return;
			}

			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool wasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/EventHub" } };
			string telemetryName = "Events";
			string telemetryNames = string.Empty;
			foreach (TEvent @event in sourceEvents)
			{
				string subTelemetryName = string.Format("{0}/{1}/{2}", @event.GetType().FullName, @event.GetIdentity(), @event.Id);
				var telemeteredEvent = @event as ITelemeteredMessage;
				if (telemeteredEvent != null)
					subTelemetryName = telemeteredEvent.TelemetryName;
				telemetryNames = string.Format("{0}{1},", telemetryNames, subTelemetryName);
			}
			if (telemetryNames.Length > 0)
				telemetryNames = telemetryNames.Substring(0, telemetryNames.Length - 1);
			telemetryProperties.Add("Events", telemetryNames);

			try
			{
				IList<string> sourceEventMessages = new List<string>();
				IList<EventData> brokeredMessages = new List<EventData>(sourceEvents.Count);
				foreach (TEvent @event in sourceEvents)
				{
					if (!AzureBusHelper.PrepareAndValidateEvent(@event, "Azure-EventHub"))
						continue;

					var brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseEvent, @event.GetType(), @event);

					brokeredMessages.Add(brokeredMessage);
					sourceEventMessages.Add(string.Format("A command was sent of type {0}.", @event.GetType().FullName));
				}

				try
				{
					if (brokeredMessages.Any())
					{
#if NETSTANDARD2_0
						EventHubPublisher.SendAsync(brokeredMessages).Wait();
#else
						EventHubPublisher.SendBatch(brokeredMessages);
#endif
					}
					else
						Logger.LogDebug("An empty collection of events to publish post validation.");
					wasSuccessfull = true;
				}
				catch (QuotaExceededException exception)
				{
					responseCode = "429";
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Events", sourceEvents } });
					throw;
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a event.", exception: exception, metaData: new Dictionary<string, object> { { "Events", sourceEvents } });
					throw;
				}

				foreach (string message in sourceEventMessages)
					Logger.LogInfo(message);

				wasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				TelemetryHelper.TrackDependency("Azure/EventHub/EventBus", "Event", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
			}
		}

		#endregion
	}
}