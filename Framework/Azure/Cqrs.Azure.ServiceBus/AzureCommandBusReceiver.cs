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
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#else
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A <see cref="ICommandReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBus<TAuthenticationToken>
		, ICommandHandlerRegistrar
		, ICommandReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// The configuration key for
		/// the number of receiver <see cref="IMessageReceiver"/> instances to create
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string NumberOfReceiversCountConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.NumberOfReceiversCount"; }
		}

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		/// <summary>
		/// Used by .NET Framework, but not .Net Core
		/// </summary>
#else
		/// <summary>
		/// The configuration key for
		/// <see cref="OnMessageOptions.MaxConcurrentCalls"/>.
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
#endif
		protected virtual string MaximumConcurrentReceiverProcessesCountConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.MaximumConcurrentReceiverProcessesCount"; }
		}

		/// <summary>
		/// The configuration key for
		/// the <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="IMessageReceiver"/> instances in the receivers
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.Topics.Subscriptions.Filter"; }
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

		static AzureCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.CommandBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
			FilterKey = new Dictionary<string, string>();
		}

		/// <summary>
		/// The debugger variable window value.
		/// </summary>
		internal string DebuggerDisplay
		{
			get
			{
				string connectionString = $"ConnectionString : {MessageBusConnectionStringConfigurationKey}";
				try
				{
					string _value = GetConnectionString();
					if (!string.IsNullOrWhiteSpace(_value))
						connectionString = string.Concat(connectionString, "=", _value);
					else
					{
						connectionString = $"ConnectionRBACSettings : ";
						connectionString = string.Concat(connectionString, "=", GetRbacConnectionSettings());
					}
				}
				catch { /* */ }
				return $"{connectionString}, PrivateTopicName : {PrivateTopicName}, PrivateTopicSubscriptionName : {PrivateTopicSubscriptionName}, PublicTopicName : {PublicTopicName}, PublicTopicSubscriptionName : {PublicTopicSubscriptionName}, FilterKey : {FilterKey}, NumberOfReceiversCount : {NumberOfReceiversCount}";
			}
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		private object lockObject = new object();
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
				lock (lockObject)
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
					SubscriptionClient client;
					string connectionString = ConnectionString;
					AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
					if (!string.IsNullOrWhiteSpace(connectionString))
						client = new SubscriptionClient(ConnectionString, topicName, topicSubscriptionName);
					else
						client = new SubscriptionClient(rbacSettings.Endpoint, topicName, topicSubscriptionName, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, $"https://login.windows.net/{rbacSettings.TenantId}"));
#else
					client = serviceBusReceivers[0];
#endif

					bool reAddRule = false;
					try
					{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
						IEnumerable<RuleDescription> rules = null;
						Task.Run(async () => {
							rules = await manager.GetRulesAsync(topicName, topicSubscriptionName);
						}).Wait();
#else
						IEnumerable<RuleDescription> rules = manager.GetRules(client.TopicPath, client.Name).ToList();
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
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
								Task.Run(async () => {
									await client.RemoveRuleAsync(ruleDescription.Name);
								}).Wait();
#else
							client.RemoveRule(ruleDescription.Name);
#endif
							}
						}
						else if (!string.IsNullOrWhiteSpace(filter))
							reAddRule = true;

						ruleDescription = rules.SingleOrDefault(rule => rule.Name == "$Default");
						// If there is a default rule and we have a rule, it will cause issues
						if (!string.IsNullOrWhiteSpace(filter) && ruleDescription != null)
						{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
							Task.Run(async () => {
								await client.RemoveRuleAsync(ruleDescription.Name);
							}).Wait();
#else
							client.RemoveRule(ruleDescription.Name);
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
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
							Task.Run(async () => {
								await client.AddRuleAsync(ruleDescription);
							}).Wait();
#else
							client.AddRule(ruleDescription);
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
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
							Task.Run(async () => {
								await client.AddRuleAsync(ruleDescription);
							}).Wait();
#else
							client.AddRule(ruleDescription);
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

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
					client.CloseAsync();
#endif
				}
			});
		}

		#endregion

		/// <summary>
		/// Register a command handler that will listen and respond to commands.
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
		/// Register a command handler that will listen and respond to commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		/// <summary>
		/// Receives a <see cref="BrokeredMessage"/> from the command bus.
		/// </summary>
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		protected virtual void ReceiveCommand(IMessageReceiver client, BrokeredMessage message)
#else
		protected virtual void ReceiveCommand(IMessageReceiver serviceBusReceiver, BrokeredMessage message)
#endif
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			// Null means it was skipped
			bool? wasSuccessfull = true;
			string telemetryName = string.Format("Cqrs/Handle/Command/{0}", message.MessageId);
			ISingleSignOnToken authenticationToken = null;
			Guid? guidAuthenticationToken = null;
			string stringAuthenticationToken = null;
			int? intAuthenticationToken = null;

			IDictionary<string, string> telemetryProperties = ExtractTelemetryProperties(message, "Azure/Servicebus");
			TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles++, telemetryProperties);
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				try
				{
					Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
					string messageBody = message.GetBodyAsString();

					ICommand<TAuthenticationToken> command = AzureBusHelper.ReceiveCommand(
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
						client
#else
						serviceBusReceiver
#endif
						, messageBody, ReceiveCommand,
						string.Format("id '{0}'", message.MessageId),
						ExtractSignature(message),
						SigningTokenConfigurationKey,
						() =>
						{
							wasSuccessfull = null;
							telemetryName = string.Format("Cqrs/Handle/Command/Skipped/{0}", message.MessageId);
							responseCode = "204";
							// Remove message from queue
							try
							{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
								client.CompleteAsync(message.SystemProperties.LockToken).Wait(1500);
#else
								message.Complete();
#endif
							}
							catch (AggregateException aggregateException)
							{
								if (aggregateException.InnerException is MessageLockLostException)
									throw new MessageLockLostException(string.Format("The lock supplied for the skipped command message '{0}' is invalid.", message.MessageId), aggregateException.InnerException);
								else
									throw;
							}
							catch (MessageLockLostException exception)
							{
								throw new MessageLockLostException(string.Format("The lock supplied for the skipped command message '{0}' is invalid.", message.MessageId), exception);
							}
							Logger.LogDebug(string.Format("A command message arrived with the id '{0}' but processing was skipped due to event settings.", message.MessageId));
							TelemetryHelper.TrackEvent("Cqrs/Handle/Command/Skipped", telemetryProperties);
						},
						() =>
						{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
							AzureBusHelper.RefreshLock(client, brokeredMessageRenewCancellationTokenSource, message, "command");
#else
							AzureBusHelper.RefreshLock(brokeredMessageRenewCancellationTokenSource, message, "command");
#endif
						}
					);

					if (wasSuccessfull != null)
					{
						if (command != null)
						{
							telemetryName = string.Format("{0}/{1}", command.GetType().FullName, command.Id);
							authenticationToken = command.AuthenticationToken as ISingleSignOnToken;
							if (AuthenticationTokenIsGuid)
								guidAuthenticationToken = command.AuthenticationToken as Guid?;
							if (AuthenticationTokenIsString)
								stringAuthenticationToken = command.AuthenticationToken as string;
							if (AuthenticationTokenIsInt)
								intAuthenticationToken = command.AuthenticationToken as int?;

							var telemeteredMessage = command as ITelemeteredMessage;
							if (telemeteredMessage != null)
								telemetryName = telemeteredMessage.TelemetryName;

							telemetryName = string.Format("Cqrs/Handle/Command/{0}", telemetryName);
						}
						// Remove message from queue
						try
						{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
							client.CompleteAsync(message.SystemProperties.LockToken).Wait(1500);
#else
							message.Complete();
#endif
						}
						catch (AggregateException aggregateException)
						{
							if (aggregateException.InnerException is MessageLockLostException)
								throw new MessageLockLostException(string.Format("The lock supplied for command '{0}' of type {1} is invalid. To avoid this issue add a '{2}.ShouldRefresh' application settings", command.Id, command.GetType().Name, command.GetType().FullName), aggregateException.InnerException);
							else
								throw;
						}
						catch (MessageLockLostException exception)
						{
							throw new MessageLockLostException(string.Format("The lock supplied for command '{0}' of type {1} is invalid. To avoid this issue add a '{2}.ShouldRefresh' application settings", command.Id, command.GetType().Name, command.GetType().FullName), exception);
						}
					}
					Logger.LogDebug(string.Format("A command message arrived and was processed with the id '{0}'.", message.MessageId));
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
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
					client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#else
					message.Abandon();
#endif
					wasSuccessfull = false;
				}
				else
				{
					Logger.LogWarning(exception.Message, exception: exception);
					try
					{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
						client.DeadLetterAsync(message.SystemProperties.LockToken, "LockLostButHandled", "The message was handled but the lock was lost.").Wait(1500);
#else
						message.DeadLetter("LockLostButHandled", "The message was handled but the lock was lost.");
#endif
					}
					catch (Exception)
					{
						// Oh well, move on.
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
						client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#else
						message.Abandon();
#endif
					}
				}
				responseCode = "599";
			}
			catch (UnAuthorisedMessageReceivedException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but was not authorised.", message.MessageId), exception: exception);
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				client.DeadLetterAsync(message.SystemProperties.LockToken, "UnAuthorisedMessageReceivedException", exception.Message).Wait(1500);
#else
				message.DeadLetter("UnAuthorisedMessageReceivedException", exception.Message);
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
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but no handlers were found to process it.", message.MessageId), exception: exception);
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				client.DeadLetterAsync(message.SystemProperties.LockToken, "NoHandlersRegisteredException", exception.Message).Wait(1500);
#else
				message.DeadLetter("NoHandlersRegisteredException", exception.Message);
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
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but no handler was found to process it.", message.MessageId), exception: exception);
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				client.DeadLetterAsync(message.SystemProperties.LockToken, "NoHandlerRegisteredException", exception.Message).Wait(1500);
#else
				message.DeadLetter("NoHandlerRegisteredException", exception.Message);
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
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#else
				message.Abandon();
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
				TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles--, telemetryProperties);

				mainStopWatch.Stop();
				if (guidAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						guidAuthenticationToken ,
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
		/// Receives a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public virtual bool? ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			return AzureBusHelper.DefaultReceiveCommand(command, Routes, "Azure-ServiceBus");
		}

		#region Overrides of AzureBus<TAuthenticationToken>

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

		#region Implementation of ICommandReceiver

		/// <summary>
		/// Starts listening and processing instances of <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public void Start()
		{
			InstantiateReceiving();

			// Configure the callback options
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
			var options = new MessageHandlerOptions((args) => { return Task.FromResult<object>(null); });
#else
			var options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
				// I think this is intentionally left out
				// , MaxConcurrentCalls = MaximumConcurrentReceiverProcessesCount
			};
#endif

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveCommand, options);
		}

#endregion
	}
}