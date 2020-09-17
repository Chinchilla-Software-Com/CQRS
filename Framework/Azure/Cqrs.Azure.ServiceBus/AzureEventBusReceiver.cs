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
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Exceptions;
using Cqrs.Messages;
#if NET452
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#endif

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
		/// the number of receiver <see cref="IMessageReceiver"/> instances to create
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string NumberOfReceiversCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.NumberOfReceiversCount"; }
		}

#if NET452
		/// <summary>
		/// The configuration key for
		/// <see cref="OnMessageOptions.MaxConcurrentCalls"/>.
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
#endif
#if NETSTANDARD2_0
		/// <summary>
		/// Used by .NET Framework, but not .Net Core
		/// </summary>
#endif
		protected virtual string MaximumConcurrentReceiverProcessesCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.MaximumConcurrentReceiverProcessesCount"; }
		}

		/// <summary>
		/// The configuration key for
		/// the <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="IMessageReceiver"/> instances in the receivers
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.Topics.Subscriptions.Filter"; }
		}

		/// <summary>
		/// The <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="IMessageReceiver"/> instances in the receivers,
		/// keyed by the topic name as there is the private and public bus
		/// </summary>
		protected IDictionary<string, string> FilterKey { get; set; }

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
		public AzureEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
			FilterKey = new Dictionary<string, string>();
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
		/// to the <see cref="IMessageReceiver"/> instances in <paramref name="serviceBusReceivers"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
		protected override void InstantiateReceiving(Manager manager, IDictionary<int, IMessageReceiver> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			base.InstantiateReceiving(manager, serviceBusReceivers, topicName, topicSubscriptionName);

			Task.Factory.StartNewSafely
			(() =>
			{
				// Because refreshing the rule can take a while, we only want to do this when the value changes
				string filter;
				if (!ConfigurationManager.TryGetSetting(FilterKeyConfigurationKey, out filter))
					return;
				if (FilterKey.ContainsKey(topicName) && FilterKey[topicName] == filter)
					return;
				FilterKey[topicName] = filter;

				// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
				// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
				// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
#if NET452
				SubscriptionClient client = serviceBusReceivers[0];
#endif
#if NETSTANDARD2_0
				// Since the IMessageReceiver and it's concrete implementation doesn't allow for the management of rules, we're creating a SubscriptionClient just to do the rules... it gets cleaned up somewhat below.
				SubscriptionClient client = new SubscriptionClient(ConnectionString, topicName, topicSubscriptionName);
#endif
				bool reAddRule = false;
				try
				{
#if NET452
					IEnumerable<RuleDescription> rules = manager.GetRules(client.TopicPath, client.Name).ToList();
#endif
#if NETSTANDARD2_0
					Task<IList<RuleDescription>> getRulesTask = manager.GetRulesAsync(client.TopicPath, client.SubscriptionName);
					getRulesTask.Wait();
					IEnumerable<RuleDescription> rules = getRulesTask.Result;
#endif
					RuleDescription ruleDescription = rules.SingleOrDefault(rule => rule.Name == "CqrsConfiguredFilter");
					if (ruleDescription != null)
					{
						var sqlFilter = ruleDescription.Filter as SqlFilter;
						if (sqlFilter == null && !string.IsNullOrWhiteSpace(filter))
							reAddRule = true;
						else if (sqlFilter != null && sqlFilter.SqlExpression != filter)
							reAddRule = true;
						if (sqlFilter != null && reAddRule)
						{
#if NET452
							client.RemoveRule("CqrsConfiguredFilter");
#endif
#if NETSTANDARD2_0
							client.RemoveRuleAsync("CqrsConfiguredFilter").Wait();
#endif
						}
					}
					else if (!string.IsNullOrWhiteSpace(filter))
						reAddRule = true;

					ruleDescription = rules.SingleOrDefault(rule => rule.Name == "$Default");
					// If there is a default rule and we have a rule, it will cause issues
					if (!string.IsNullOrWhiteSpace(filter) && ruleDescription != null)
					{
#if NET452
						client.RemoveRule("$Default");
#endif
#if NETSTANDARD2_0
						client.RemoveRuleAsync("$Default").Wait();
#endif
					}
					// If we don't have a rule and there is no longer a default rule, it will cause issues
					else if (string.IsNullOrWhiteSpace(filter) && !rules.Any())
					{
						ruleDescription = new RuleDescription
						(
							"$Default",
							new SqlFilter("1=1")
						);
#if NET452
						client.AddRule(ruleDescription);
#endif
#if NETSTANDARD2_0
						client.AddRuleAsync(ruleDescription).Wait();
#endif
					}
				}
				catch (AggregateException ex)
				{
					if (!(ex.InnerException is MessagingEntityNotFoundException))
						throw;
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
#if NET452
						client.AddRule(ruleDescription);
#endif
#if NETSTANDARD2_0
						client.AddRuleAsync(ruleDescription).Wait();
#endif
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

#if NETSTANDARD2_0
				client.CloseAsync();
#endif
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
#if NET452
		protected virtual void ReceiveEvent(IMessageReceiver serviceBusReceiver, BrokeredMessage message)
#endif
#if NETSTANDARD2_0
		protected virtual void ReceiveEvent(IMessageReceiver client, BrokeredMessage message)
#endif
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			// Null means it was skipped
			bool? wasSuccessfull = true;
			string telemetryName = string.Format("Cqrs/Handle/Event/{0}", message.MessageId);
			ISingleSignOnToken authenticationToken = null;
			Guid? guidAuthenticationToken = null;
			string stringAuthenticationToken = null;
			int? intAuthenticationToken = null;

			IDictionary<string, string> telemetryProperties = ExtractTelemetryProperties(message, "Azure/Servicebus");
			TelemetryHelper.TrackMetric("Cqrs/Handle/Event", CurrentHandles++, telemetryProperties);
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				try
				{
					Logger.LogDebug(string.Format("An event message arrived with the id '{0}'.", message.MessageId));
					string messageBody = message.GetBodyAsString();

					IEvent<TAuthenticationToken> @event = AzureBusHelper.ReceiveEvent(
#if NET452
						serviceBusReceiver
#endif
#if NETSTANDARD2_0
						client
#endif
						, messageBody, ReceiveEvent,
						string.Format("id '{0}'", message.MessageId),
						ExtractSignature(message),
						SigningTokenConfigurationKey,
						() =>
						{
							wasSuccessfull = null;
							telemetryName = string.Format("Cqrs/Handle/Event/Skipped/{0}", message.MessageId);
							responseCode = "204";
							// Remove message from queue
							try
							{
#if NET452
								message.Complete();
#endif
#if NETSTANDARD2_0
								client.CompleteAsync(message.SystemProperties.LockToken).Wait(1500);
#endif
							}
							catch (AggregateException aggregateException)
							{
								if (aggregateException.InnerException is MessageLockLostException)
									throw new MessageLockLostException(string.Format("The lock supplied for the skipped event message '{0}' is invalid.", message.MessageId), aggregateException.InnerException);
								else
									throw;
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
#if NET452
							AzureBusHelper.RefreshLock(brokeredMessageRenewCancellationTokenSource, message, "event");
#endif
#if NETSTANDARD2_0
							AzureBusHelper.RefreshLock(client, brokeredMessageRenewCancellationTokenSource, message, "event");
#endif
						}
					);

					if (wasSuccessfull != null)
					{
						if (@event != null)
						{
							telemetryName = string.Format("{0}/{1}/{2}", @event.GetType().FullName, @event.GetIdentity(), @event.Id);
							authenticationToken = @event.AuthenticationToken as ISingleSignOnToken;
							if (AuthenticationTokenIsGuid)
								guidAuthenticationToken = @event.AuthenticationToken as Guid?;
							if (AuthenticationTokenIsString)
								stringAuthenticationToken = @event.AuthenticationToken as string;
							if (AuthenticationTokenIsInt)
								intAuthenticationToken = @event.AuthenticationToken as int?;

							var telemeteredMessage = @event as ITelemeteredMessage;
							if (telemeteredMessage != null)
								telemetryName = telemeteredMessage.TelemetryName;

							telemetryName = string.Format("Cqrs/Handle/Event/{0}", telemetryName);
						}
						// Remove message from queue
						try
						{
#if NET452
							message.Complete();
#endif
#if NETSTANDARD2_0
							client.CompleteAsync(message.SystemProperties.LockToken).Wait(1500);
#endif
						}
						catch (AggregateException aggregateException)
						{
							if (aggregateException.InnerException is MessageLockLostException)
								throw new MessageLockLostException(string.Format("The lock supplied for event '{0}' of type {1} is invalid.", @event.Id, @event.GetType().Name), aggregateException.InnerException);
							else
								throw;
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
				catch (AggregateException aggregateException)
				{
					throw aggregateException.InnerException;
				}
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
#if NET452
					message.Abandon();
#endif
#if NETSTANDARD2_0
					client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#endif
					wasSuccessfull = false;
				}
				else
				{
					Logger.LogWarning(exception.Message, exception: exception);
					try
					{
#if NET452
						message.DeadLetter("LockLostButHandled", "The message was handled but the lock was lost.");
#endif
#if NETSTANDARD2_0
						client.DeadLetterAsync(message.SystemProperties.LockToken, "LockLostButHandled", "The message was handled but the lock was lost.").Wait(1500);
#endif
					}
					catch (Exception)
					{
						// Oh well, move on.
#if NET452
						message.Abandon();
#endif
#if NETSTANDARD2_0
						client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#endif
					}
				}
				responseCode = "599";
			}
			catch (UnAuthorisedMessageReceivedException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but was not authorised.", message.MessageId), exception: exception);
#if NET452
				message.DeadLetter("UnAuthorisedMessageReceivedException", exception.Message);
#endif
#if NETSTANDARD2_0
				client.DeadLetterAsync(message.SystemProperties.LockToken, "UnAuthorisedMessageReceivedException", exception.Message).Wait(1500);
#endif
				wasSuccessfull = false;
				responseCode = "401";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
			}
			catch (NoHandlersRegisteredException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but no handlers were found to process it.", message.MessageId), exception: exception);
#if NET452
				message.DeadLetter("NoHandlersRegisteredException", exception.Message);
#endif
#if NETSTANDARD2_0
				client.DeadLetterAsync(message.SystemProperties.LockToken, "NoHandlersRegisteredException", exception.Message).Wait(1500);
#endif
				wasSuccessfull = false;
				responseCode = "501";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
			}
			catch (NoHandlerRegisteredException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but no handler was found to process it.", message.MessageId), exception: exception);
#if NET452
				message.DeadLetter("NoHandlerRegisteredException", exception.Message);
#endif
#if NETSTANDARD2_0
				client.DeadLetterAsync(message.SystemProperties.LockToken, "NoHandlerRegisteredException", exception.Message).Wait(1500);
#endif
				wasSuccessfull = false;
				responseCode = "501";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
			}
			catch (Exception exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
#if NET452
				message.Abandon();
#endif
#if NETSTANDARD2_0
				client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#endif
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
				if (guidAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						guidAuthenticationToken,
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull == null || wasSuccessfull.Value,
						telemetryProperties
					);
				else if (intAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						intAuthenticationToken,
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull == null || wasSuccessfull.Value,
						telemetryProperties
					);
				else if (stringAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						stringAuthenticationToken,
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull == null || wasSuccessfull.Value,
						telemetryProperties
					);
				else
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
#if NET452
			var options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
				// I think this is intentionally left out
				// , MaxConcurrentCalls = MaximumConcurrentReceiverProcessesCount
			};
#endif
#if NETSTANDARD2_0
			var options = new MessageHandlerOptions((args) => { return Task.FromResult<object>(null); });
#endif

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveEvent, options);
		}

		#endregion
	}
}