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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using cdmdotnet.Logging;
using Cqrs.Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A <see cref="IEventReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureEventBusReceiver<TAuthenticationToken>
		: AzureEventBus<TAuthenticationToken>
		, IEventHandlerRegistrar
		, IEventReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// The configuration key for
		/// the number of receiver <see cref="SubscriptionClient"/> instances to create
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string NumberOfReceiversCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.NumberOfReceiversCount"; }
		}

		/// <summary>
		/// The configuration key for
		/// <see cref="OnMessageOptions.MaxConcurrentCalls"/>.
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string MaximumConcurrentReceiverProcessesCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.MaximumConcurrentReceiverProcessesCount"; }
		}

		/// <summary>
		/// The configuration key for
		/// the <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="SubscriptionClient"/> instances in the receivers
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.TopicName.SubscriptionName.Filter"; }
		}

		/// <summary>
		/// The <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="SubscriptionClient"/> instances in the receivers
		/// </summary>
		protected string FilterKey { get; set; }

		// ReSharper disable StaticMemberInGenericType
		/// <summary>
		/// Gets the <see cref="RouteManager"/>.
		/// </summary>
		public static RouteManager Routes { get; private set; }

		/// <summary>
		/// The number of handles currently being executed.
		/// </summary>
		protected static long CurrentHandles { get; set; }
		// ReSharper restore StaticMemberInGenericType

		static AzureEventBusReceiver()
		{
			Routes = new RouteManager();
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureEventBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		/// <summary>
		/// The debugger variable window value.
		/// </summary>
		internal string DebuggerDisplay
		{
			get
			{
				string connectionString = string.Format("ConnectionString : {0}", MessageBusConnectionStringConfigurationKey);
				try
				{
					connectionString = string.Concat(connectionString, "=", GetConnectionString());
				}
				catch { /* */ }
				return string.Format(CultureInfo.InvariantCulture, "{0}, PrivateTopicName : {1}, PrivateTopicSubscriptionName : {2}, PublicTopicName : {3}, PublicTopicSubscriptionName : {4}, FilterKey : {5}, NumberOfReceiversCount : {6}",
					connectionString, PrivateTopicName, PrivateTopicSubscriptionName, PublicTopicName, PublicTopicSubscriptionName, FilterKey, NumberOfReceiversCount);
			}
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// Calls <see cref="AzureServiceBus{TAuthenticationToken}.InstantiateReceiving()"/>
		/// then uses a <see cref="Task"/> to apply the <see cref="FilterKey"/> as a <see cref="RuleDescription"/>
		/// to the <see cref="SubscriptionClient"/> instances in <paramref name="serviceBusReceivers"/>.
		/// </summary>
		/// <param name="namespaceManager">The <see cref="NamespaceManager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="SubscriptionClient"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
		protected override void InstantiateReceiving(NamespaceManager namespaceManager, IDictionary<int, SubscriptionClient> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			base.InstantiateReceiving(namespaceManager, serviceBusReceivers, topicName, topicSubscriptionName);

			Task.Factory.StartNewSafely
			(() =>
			{
				// Because refreshing the rule can take a while, we only want to do this when the value changes
				string filter;
				if (!ConfigurationManager.TryGetSetting(FilterKeyConfigurationKey, out filter))
					return;
				if (FilterKey == filter)
					return;
				FilterKey = filter;

				// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
				// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
				// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
				SubscriptionClient client = serviceBusReceivers[0];
				bool reAddRule = false;
				try
				{
					IEnumerable<RuleDescription> rules = namespaceManager.GetRules(client.TopicPath, client.Name).ToList();
					RuleDescription ruleDescription = rules.SingleOrDefault(rule => rule.Name == "CqrsConfiguredFilter");
					if (ruleDescription != null)
					{
						var sqlFilter = ruleDescription.Filter as SqlFilter;
						if (sqlFilter == null && !string.IsNullOrWhiteSpace(filter))
							reAddRule = true;
						else if (sqlFilter != null && sqlFilter.SqlExpression != filter)
							reAddRule = true;
						if (sqlFilter != null && reAddRule)
							client.RemoveRule("CqrsConfiguredFilter");
					}
					else if (!string.IsNullOrWhiteSpace(filter))
						reAddRule = true;

					ruleDescription = rules.SingleOrDefault(rule => rule.Name == "$Default");
					// If there is a default rule and we have a rule, it will cause issues
					if (!string.IsNullOrWhiteSpace(filter) && ruleDescription != null)
						client.RemoveRule("$Default");
					// If we don't have a rule and there is no longer a default rule, it will cause issues
					else if (string.IsNullOrWhiteSpace(filter) && !rules.Any())
					{
						ruleDescription = new RuleDescription
						(
							"$Default",
							new SqlFilter("1=1")
						);
						client.AddRule(ruleDescription);
					}
				}
				catch (MessagingEntityNotFoundException)
				{
				}

				if (!reAddRule)
					return;

				int loopCounter = 0;
				while (loopCounter < 10)
				{
					try
					{
						RuleDescription ruleDescription = new RuleDescription
						(
							"CqrsConfiguredFilter",
							new SqlFilter(filter)
						);
						client.AddRule(ruleDescription);
						break;
					}
					catch (MessagingEntityAlreadyExistsException exception)
					{
						loopCounter++;
						// Still waiting for the delete to complete
						Thread.Sleep(1000);
						if (loopCounter == 9)
						{
							Logger.LogError("Setting the filter failed as it already exists.", exception: exception);
							TelemetryHelper.TrackException(exception);
						}
					}
					catch (Exception exception)
					{
						Logger.LogError("Setting the filter failed.", exception: exception);
						TelemetryHelper.TrackException(exception);
						break;
					}
				}
			});
		}

		#endregion

		/// <summary>
		/// Register an event handler that will listen and respond to events.
		/// </summary>
		/// <remarks>
		/// In many cases the <paramref name="targetedType"/> will be the handler class itself, what you actually want is the target of what is being updated.
		/// </remarks>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterHandler(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register an event handler that will listen and respond to events.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = false)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		/// <summary>
		/// Register an event handler that will listen and respond to all events.
		/// </summary>
		public void RegisterGlobalEventHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterGlobalEventHandler(TelemetryHelper, Routes, handler, holdMessageLock);
		}

		/// <summary>
		/// Receives a <see cref="BrokeredMessage"/> from the event bus.
		/// </summary>
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
						telemetryName = string.Format("{0}/{1}/{2}", @event.GetType().FullName, @event.GetIdentity(), @event.Id);
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
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
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

		/// <summary>
		/// Receives a <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
		/// </summary>
		public virtual bool? ReceiveEvent(IEvent<TAuthenticationToken> @event)
		{
			return AzureBusHelper.DefaultReceiveEvent(@event, Routes, "Azure-ServiceBus");
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// Returns <see cref="NumberOfReceiversCountConfigurationKey"/> from <see cref="IConfigurationManager"/> 
		/// if no value is set, returns <see cref="AzureBus{TAuthenticationToken}.DefaultNumberOfReceiversCount"/>.
		/// </summary>
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

		/// <summary>
		/// Returns <see cref="MaximumConcurrentReceiverProcessesCountConfigurationKey"/> from <see cref="IConfigurationManager"/> 
		/// if no value is set, returns <see cref="AzureBus{TAuthenticationToken}.DefaultMaximumConcurrentReceiverProcessesCount"/>.
		/// </summary>
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

		#region Implementation of IEventReceiver

		/// <summary>
		/// Starts listening and processing instances of <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
		/// </summary>
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

		#endregion
	}
}