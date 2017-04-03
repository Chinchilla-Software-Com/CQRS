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
using System.Threading;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusReceiver<TAuthenticationToken>
		: AzureEventBus<TAuthenticationToken>
		, IEventHandlerRegistrar
		, IEventReceiver<TAuthenticationToken>
	{
		protected virtual string NumberOfReceiversCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.NumberOfReceiversCount"; }
		}

		protected virtual string MaximumConcurrentReceiverProcessesCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.MaximumConcurrentReceiverProcessesCount"; }
		}

		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.TopicName.SubscriptionName.Filter"; }
		}

		// ReSharper disable StaticMemberInGenericType
		protected static RouteManager Routes { get; private set; }

		protected static long CurrentHandles { get; set; }
		// ReSharper restore StaticMemberInGenericType

		static AzureEventBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		public void Start()
		{
			InstantiateReceiving();

			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1),
				MaxConcurrentCalls = MaximumConcurrentReceiverProcessesCount
			};

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveEvent, options);
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override void InstantiateReceiving(IDictionary<int, SubscriptionClient> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			base.InstantiateReceiving(serviceBusReceivers, topicName, topicSubscriptionName);

			// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
			// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
			// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
			SubscriptionClient client = serviceBusReceivers[0];
			try
			{
				client.RemoveRule("CqrsConfiguredFilter");
			}
			catch (MessagingEntityNotFoundException)
			{
			}

			string filter = ConfigurationManager.GetSetting(FilterKeyConfigurationKey);
			if (!string.IsNullOrWhiteSpace(filter))
			{
				RuleDescription ruleDescription = new RuleDescription
				(
					"CqrsConfiguredFilter",
					new SqlFilter(filter)
				);
				client.AddRuleAsync(ruleDescription);
			}
		}

		#endregion

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterHandler(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
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
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			// Null means it was skipped
			bool? wasSuccessfull = true;
			string telemetryName = string.Format("Cqrs/Handle/Event/{0}", message.MessageId);
			ISingleSignOnToken authenticationToken = null;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			object value;
			if (message.Properties.TryGetValue("Type", out value))
				telemetryProperties.Add("MessageType", value.ToString());
			TelemetryHelper.TrackMetric("Cqrs/Handle/Event", CurrentHandles++, telemetryProperties);
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				Logger.LogDebug(string.Format("An event message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();

				IEvent<TAuthenticationToken> @event = AzureBusHelper.ReceiveEvent(messageBody, ReceiveEvent,
					string.Format("id '{0}'", message.MessageId),
					() =>
					{
						wasSuccessfull = null;
						telemetryName = string.Format("Cqrs/Handle/Event/Skipped/{0}", message.MessageId);
						responseCode = "204";
						// Remove message from queue
						try
						{
							message.Complete();
						}
						catch (MessageLockLostException exception)
						{
							throw new MessageLockLostException(string.Format("The lock supplied for the skipped event message '{0}' is invalid.", message.MessageId), exception);
						}
						Logger.LogDebug(string.Format("An event message arrived with the id '{0}' but processing was skipped due to event settings.", message.MessageId));
						TelemetryHelper.TrackEvent("Cqrs/Handle/Event/Skipped", telemetryProperties);
					},
					() =>
					{
						AzureBusHelper.RefreshLock(brokeredMessageRenewCancellationTokenSource, message, "event");
					}
				);

				if (wasSuccessfull != null)
				{
					if (@event != null)
					{
						telemetryName = string.Format("{0}/{1}", @event.GetType().FullName, @event.Id);
						authenticationToken = @event.AuthenticationToken as ISingleSignOnToken;

						var telemeteredMessage = @event as ITelemeteredMessage;
						if (telemeteredMessage != null)
							telemetryName = telemeteredMessage.TelemetryName;

						telemetryName = string.Format("Cqrs/Handle/Event/{0}", telemetryName);
					}
					// Remove message from queue
					try
					{
						message.Complete();
					}
					catch (MessageLockLostException exception)
					{
						throw new MessageLockLostException(string.Format("The lock supplied for event '{0}' of type {1} is invalid.", @event.Id, @event.GetType().Name), exception);
					}
				}
				Logger.LogDebug(string.Format("An event message arrived and was processed with the id '{0}'.", message.MessageId));

				IList<IEvent<TAuthenticationToken>> events;
				if (EventWaits.TryGetValue(@event.CorrelationId, out events))
					events.Add(@event);
			}
			catch (MessageLockLostException exception)
			{
				IDictionary<string, string> subTelemetryProperties = new Dictionary<string, string>(telemetryProperties);
				subTelemetryProperties.Add("TimeTaken", mainStopWatch.Elapsed.ToString());
				TelemetryHelper.TrackException(exception, null, subTelemetryProperties);
				if (ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete)
				{
					Logger.LogError(exception.Message, exception: exception);
					// Indicates a problem, unlock message in queue
					message.Abandon();
					wasSuccessfull = false;
				}
				else
				{
					Logger.LogWarning(exception.Message, exception: exception);
					try
					{
						message.DeadLetter("LockLostButHandled", "The message was handled but the lock was lost.");
					}
					catch (Exception)
					{
						// Oh well, move on.
						message.Abandon();
					}
				}
				responseCode = "599";
			}
			catch (Exception exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
				wasSuccessfull = false;
				responseCode = "500";
			}
			finally
			{
				// Cancel the lock of renewing the task
				brokeredMessageRenewCancellationTokenSource.Cancel();
				TelemetryHelper.TrackMetric("Cqrs/Handle/Event", CurrentHandles--, telemetryProperties);

				mainStopWatch.Stop();
				TelemetryHelper.TrackRequest
				(
					telemetryName,
					authenticationToken,
					startedAt,
					mainStopWatch.Elapsed,
					responseCode,
					wasSuccessfull == null || wasSuccessfull.Value,
					telemetryProperties
				);

				TelemetryHelper.Flush();
			}
		}

		public virtual void ReceiveEvent(IEvent<TAuthenticationToken> @event)
		{
			AzureBusHelper.DefaultReceiveEvent(@event, Routes, "Azure-ServiceBus");
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override int GetCurrentNumberOfReceiversCount()
		{
			string numberOfReceiversCountValue;
			int numberOfReceiversCount;
			if (ConfigurationManager.TryGetSetting(NumberOfReceiversCountConfigurationKey, out numberOfReceiversCountValue) && !string.IsNullOrWhiteSpace(numberOfReceiversCountValue))
			{
				if (!int.TryParse(numberOfReceiversCountValue, out numberOfReceiversCount))
					numberOfReceiversCount = DefaultNumberOfReceiversCount;
			}
			else
				numberOfReceiversCount = DefaultNumberOfReceiversCount;
			return numberOfReceiversCount;
		}

		protected override int GetCurrentMaximumConcurrentReceiverProcessesCount()
		{
			string maximumConcurrentReceiverProcessesCountValue;
			int maximumConcurrentReceiverProcessesCount;
			if (ConfigurationManager.TryGetSetting(MaximumConcurrentReceiverProcessesCountConfigurationKey, out maximumConcurrentReceiverProcessesCountValue) && !string.IsNullOrWhiteSpace(maximumConcurrentReceiverProcessesCountValue))
			{
				if (!int.TryParse(maximumConcurrentReceiverProcessesCountValue, out maximumConcurrentReceiverProcessesCount))
					maximumConcurrentReceiverProcessesCount = DefaultMaximumConcurrentReceiverProcessesCount;
			}
			else
				maximumConcurrentReceiverProcessesCount = DefaultMaximumConcurrentReceiverProcessesCount;
			return maximumConcurrentReceiverProcessesCount;
		}

		#endregion
	}
}