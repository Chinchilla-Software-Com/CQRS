#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;
using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using OldManager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using OldIMessageReceiver = Microsoft.Azure.ServiceBus.Core.IMessageReceiver;
using OldTopicClient = Microsoft.Azure.ServiceBus.TopicClient;
#else
using OldManager = Microsoft.ServiceBus.NamespaceManager;
using OldIMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
using OldTopicClient = Microsoft.ServiceBus.Messaging.TopicClient;
#endif


namespace Cqrs.Azure.Functions.ServiceBus
{
	/// <summary>
	/// An <see cref="AzureBus{TAuthenticationToken}"/> that uses Azure Service Bus hosted in Azure Functions.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <remarks>
	/// https://markheath.net/post/migrating-to-new-servicebus-sdk
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#receive-messages-from-the-subscription
	/// https://stackoverflow.com/questions/47427361/azure-service-bus-read-messages-sent-by-net-core-2-with-brokeredmessage-getbo
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
	/// </remarks>
	public abstract class AzureFunctionServiceBus<TAuthenticationToken>
		: AzureServiceBus<TAuthenticationToken>
	{
		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// Gets the private <see cref="ServiceBusSender"/> publisher.
		/// </summary>
		protected ServiceBusSender PrivateServiceBusPublisher { get; private set; }

		/// <summary>
		/// Gets the public <see cref="ServiceBusSender"/> publisher.
		/// </summary>
		protected ServiceBusSender PublicServiceBusPublisher { get; private set; }

		/// <summary>
		/// Gets the private <see cref="ServiceBusProcessor"/> receivers.
		/// </summary>
		protected IDictionary<int, ServiceBusProcessor> PrivateServiceBusReceivers { get; private set; }

		/// <summary>
		/// Gets the public <see cref="ServiceBusProcessor"/> receivers.
		/// </summary>
		protected IDictionary<int, ServiceBusProcessor> PublicServiceBusReceivers { get; private set; }

		/// <summary>
		/// The <see cref="Func{ProcessMessageEventArgs}">handler</see> used for <see cref="ServiceBusProcessor.OnProcessMessageAsync(ProcessMessageEventArgs)"/> on each receiver.
		/// </summary>
		protected virtual Func<ProcessMessageEventArgs, Task> FunctionReceiverMessageHandler { get; set; }

		/// <summary>
		/// The <see cref="ServiceBusProcessorOptions" /> used.
		/// </summary>
		protected virtual ServiceBusProcessorOptions FunctionReceiverMessageHandlerOptions { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureServiceBus{TAuthenticationToken}"/>
		/// </summary>
		protected AzureFunctionServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, isAPublisher)
		{
		}

		/// <summary>
		/// This method should not be called
		/// </summary>
		[Obsolete("This method should not be called.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void Instantiate()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("This method should not be called.");
		}

		/// <summary>
		/// Instantiate receiving on this bus by
		/// calling <see cref="CheckPrivateTopicExists"/> and <see cref="CheckPublicTopicExists"/>
		/// then InstantiateReceiving for private and public topics,
		/// calls <see cref="CleanUpDeadLetters"/> for the private and public topics,
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiateReceiving()
		{
			// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
			// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
			// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
			ServiceBusClient client;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;

			if (!string.IsNullOrWhiteSpace(connectionString))
				client = new ServiceBusClient(ConnectionString);
			else
			{
				var credentials = new ClientSecretCredential(rbacSettings.TenantId, rbacSettings.ApplicationId, rbacSettings.ClientKey);
				client = new ServiceBusClient(rbacSettings.Endpoint, credentials);
			}

			Task.Run(async () => {
				Manager manager;
				if (!string.IsNullOrWhiteSpace(connectionString))
					manager = new Manager(ConnectionString);
				else
				{
					var credentials = new ClientSecretCredential(rbacSettings.TenantId, rbacSettings.ApplicationId, rbacSettings.ClientKey);
					manager = new Manager(rbacSettings.Endpoint, credentials);
				}

				await CheckPrivateTopicExistsAsync(manager);
				await CheckPublicTopicExistsAsync(manager);
			}).Wait();

			try
			{
				Task.Run(async () => {
					await InstantiateReceivingAsync(client, PrivateServiceBusReceivers, PrivateTopicName, PrivateTopicSubscriptionName);
				}).Wait();
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the private Service Bus receivers may be invalid.", exception);
			}
			try
			{
				Task.Run(async () => {
					await InstantiateReceivingAsync(client, PublicServiceBusReceivers, PublicTopicName, PublicTopicSubscriptionName);
				}).Wait();
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the public Service Bus receivers may be invalid.", exception);
			}

			bool enableDeadLetterCleanUp;
			string enableDeadLetterCleanUpValue = ConfigurationManager.GetSetting("Cqrs.Azure.Servicebus.EnableDeadLetterCleanUp");
			if (bool.TryParse(enableDeadLetterCleanUpValue, out enableDeadLetterCleanUp) && enableDeadLetterCleanUp)
			{
				Task.Run(async () => {
					await CleanUpDeadLettersAsync(client, PrivateTopicName, PrivateTopicSubscriptionName);
					await CleanUpDeadLettersAsync(client, PublicTopicName, PublicTopicSubscriptionName);
				}).Wait();
			}

			// If this is also a publisher, then it will the check over there and that will handle this
			// we only need to check one of these
			if (PublicServiceBusPublisher != null)
				return;

			StartSettingsChecking();
		}

		/// <summary>
		/// This method should not be called
		/// </summary>
		[Obsolete("This method should not be called.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void InstantiateReceiving(OldManager manager, IDictionary<int, OldIMessageReceiver> serviceBusReceivers, string topicName, string topicSubscriptionName)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("This method should not be called.");
		}
		/// <summary>
		/// Creates a single <see cref="ServiceBusProcessor"/>.
		/// </summary>
		/// <param name="client">The <see cref="ServiceBusClient"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="ServiceBusProcessor"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
		protected virtual async Task InstantiateReceivingAsync(ServiceBusClient client, IDictionary<int, ServiceBusProcessor> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			ServiceBusProcessor serviceBusReceiver = client.CreateProcessor(topicName, topicSubscriptionName, new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, Identifier = Logger.LoggerSettings.ModuleName, AutoCompleteMessages = false, MaxConcurrentCalls = MaximumConcurrentReceiverProcessesCount, MaxAutoLockRenewalDuration = new TimeSpan(0, 5, 0) });

			if (serviceBusReceivers.ContainsKey(0))
			{
				await serviceBusReceivers[0].CloseAsync();
				await serviceBusReceivers[0].DisposeAsync();
				serviceBusReceivers[0] = serviceBusReceiver;
			}
			else
				serviceBusReceivers.Add(0, serviceBusReceiver);
			serviceBusReceivers.Add(0, serviceBusReceiver);
		}

		/// <summary>
		/// Replaced with <see cref="CheckPrivateTopicExistsAsync"/>
		/// </summary>
		[Obsolete("Replaced with CheckPrivateTopicExistsAsync.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void CheckPrivateTopicExists(OldManager manager, bool createSubscriptionIfNotExists = true)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with CheckPrivateTopicExistsAsync.");
		}
		/// <summary>
		/// Checks if the private topic and subscription name exists as per <see cref="AzureServiceBus{TAuthenticationToken}.PrivateTopicName"/> and <see cref="AzureServiceBus{TAuthenticationToken}.PrivateTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="ServiceBusClient"/>.</param>
		/// <param name="createSubscriptionIfNotExists">Create a subscription if there isn't one</param>
		protected virtual async Task CheckPrivateTopicExistsAsync(Manager manager, bool createSubscriptionIfNotExists = true)
		{
			await CheckTopicExistsAsync(manager, PrivateTopicName = ConfigurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName, PrivateTopicSubscriptionName = ConfigurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName, createSubscriptionIfNotExists);
		}

		/// <summary>
		/// Replaced with <see cref="CheckPublicTopicExistsAsync"/>
		/// </summary>
		[Obsolete("Replaced with CheckPublicTopicExistsAsync.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void CheckPublicTopicExists(OldManager manager, bool createSubscriptionIfNotExists = true)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with CheckPublicTopicExistsAsync.");
		}
		/// <summary>
		/// Checks if the public topic and subscription name exists as per <see cref="AzureServiceBus{TAuthenticationToken}.PublicTopicName"/> and <see cref="AzureServiceBus{TAuthenticationToken}.PublicTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="ServiceBusClient"/>.</param>
		/// <param name="createSubscriptionIfNotExists">Create a subscription if there isn't one</param>
		protected virtual async Task CheckPublicTopicExistsAsync(Manager manager, bool createSubscriptionIfNotExists = true)
		{
			await CheckTopicExistsAsync(manager, PublicTopicName = ConfigurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName, PublicTopicSubscriptionName = ConfigurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName, createSubscriptionIfNotExists);
		}

		/// <summary>
		/// Replaced with <see cref="CheckTopicExistsAsync"/>
		/// </summary>
		[Obsolete("Replaced with CheckTopicExistsAsync.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void CheckTopicExists(OldManager manager, string topicName, string subscriptionName, bool createSubscriptionIfNotExists = true)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with CheckTopicExistsAsync.");
		}
		/// <summary>
		/// Checks if a topic by the provided <paramref name="topicName"/> exists and
		/// Checks if a subscription name by the provided <paramref name="subscriptionName"/> exists.
		/// </summary>
		protected virtual async Task CheckTopicExistsAsync(Manager manager, string topicName, string subscriptionName, bool createSubscriptionIfNotExists = true)
		{
			// Configure Queue Settings
			var eventTopicDescription = new CreateTopicOptions(topicName)
			{
				MaxSizeInMegabytes = 5120,
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true,
				SupportOrdering = true
			};

			bool topicExists = await manager.TopicExistsAsync(topicName);
			if (!topicExists)
			{
				TopicProperties createdTopic = await manager.CreateTopicAsync(eventTopicDescription);
				Logger.LogInfo($"Created topic '{createdTopic.Name}'");
			}

			if (createSubscriptionIfNotExists)
			{
				bool subscriptionExists = await manager.SubscriptionExistsAsync(topicName, subscriptionName);
				if (!subscriptionExists)
				{
					var subscriptionDescription = new CreateSubscriptionOptions(topicName, subscriptionName)
					{
						DefaultMessageTimeToLive = eventTopicDescription.DefaultMessageTimeToLive,
						EnableBatchedOperations = eventTopicDescription.EnableBatchedOperations,
						DeadLetteringOnMessageExpiration = true,
						LockDuration = new TimeSpan(0, 5, 0)
					};
					SubscriptionProperties createdSubscription = await manager.CreateSubscriptionAsync(subscriptionDescription);
					Logger.LogInfo($"Created subscription '{createdSubscription.SubscriptionName}' on topic '{createdSubscription.TopicName}'");
				}
			}
		}

		/// <summary>
		/// Replaced with <see cref="RegisterReceiverMessageHandlerAsync"/>
		/// </summary>
		[Obsolete("Replaced with RegisterReceiverMessageHandler.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		protected override void RegisterReceiverMessageHandler(Action<Microsoft.Azure.ServiceBus.Core.IMessageReceiver, Microsoft.Azure.ServiceBus.Message> receiverMessageHandler, Microsoft.Azure.ServiceBus.MessageHandlerOptions receiverMessageHandlerOptions)
#else
		protected override void RegisterReceiverMessageHandler(Action<Microsoft.ServiceBus.Messaging.SubscriptionClient, Microsoft.ServiceBus.Messaging.BrokeredMessage> receiverMessageHandler, Microsoft.ServiceBus.Messaging.OnMessageOptions receiverMessageHandlerOptions)
#endif
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with RegisterReceiverMessageHandler.");
		}
		/// <summary>
		/// Registers the provided <paramref name="receiverMessageHandler"/> with the provided <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
		protected async virtual Task RegisterReceiverMessageHandlerAsync(Func<ProcessMessageEventArgs, Task> receiverMessageHandler, ServiceBusProcessorOptions receiverMessageHandlerOptions)
		{
			await StoreReceiverMessageHandlerAsync(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();

			await Task.CompletedTask;
		}

		/// <summary>
		/// Replaced with <see cref="StoreReceiverMessageHandlerAsync"/>
		/// </summary>
		[Obsolete("Replaced with StoreReceiverMessageHandlerAsync.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		protected override void StoreReceiverMessageHandler(Action<Microsoft.Azure.ServiceBus.Core.IMessageReceiver, Microsoft.Azure.ServiceBus.Message> receiverMessageHandler, Microsoft.Azure.ServiceBus.MessageHandlerOptions receiverMessageHandlerOptions)
#else
		protected override void StoreReceiverMessageHandler(Action<Microsoft.ServiceBus.Messaging.SubscriptionClient, Microsoft.ServiceBus.Messaging.BrokeredMessage> receiverMessageHandler, Microsoft.ServiceBus.Messaging.OnMessageOptions receiverMessageHandlerOptions)
#endif
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with StoreReceiverMessageHandlerAsync.");
		}
		/// <summary>
		/// Stores the provided <paramref name="receiverMessageHandler"/> and <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
		protected async virtual Task StoreReceiverMessageHandlerAsync(Func<ProcessMessageEventArgs, Task> receiverMessageHandler, ServiceBusProcessorOptions receiverMessageHandlerOptions)
		{
			FunctionReceiverMessageHandler = receiverMessageHandler;
			FunctionReceiverMessageHandlerOptions = receiverMessageHandlerOptions;

			await Task.CompletedTask;
		}

		/// <summary>
		/// Replaced with <see cref="CleanUpDeadLettersAsync"/>
		/// </summary>
		[Obsolete("Replaced with CleanUpDeadLettersAsync.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override CancellationTokenSource CleanUpDeadLetters(string topicName, string topicSubscriptionName)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with CleanUpDeadLettersAsync.");
		}
		/// <summary>
		/// Using a <see cref="Task"/>, clears all dead letters from the topic and subscription of the 
		/// provided <paramref name="topicName"/> and <paramref name="topicSubscriptionName"/>.
		/// </summary>
		/// <param name="client">The <see cref="ServiceBusClient"/></param>
		/// <param name="topicName">The name of the topic.</param>
		/// <param name="topicSubscriptionName">The name of the subscription.</param>
		/// <returns></returns>
		protected async virtual Task<CancellationTokenSource> CleanUpDeadLettersAsync(ServiceBusClient client, string topicName, string topicSubscriptionName)
		{
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			int lockIssues = 0;

			Func<ServiceBusReceiver, ServiceBusReceivedMessage, IMessage, Task> leaveDeadlLetterInQueue = async (receiver, deadLetterBrokeredMessage, deadLetterMessage) =>
			{
				// Remove message from queue
				try
				{
					await receiver.AbandonMessageAsync(deadLetterBrokeredMessage);
					lockIssues = 0;
				}
				catch (ServiceBusException ex)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId), "Cqrs.Azure.Functions.ServiceBus.CleanUpDeadLettersAsync.LeaveDeadlLetterInQueue", exception: ex);
				}
				Logger.LogDebug(string.Format("A dead-letter message of type {0} arrived with the id '{1}' but left in the queue due to settings.", deadLetterMessage.GetType().FullName, deadLetterBrokeredMessage.MessageId), "Cqrs.Azure.Functions.ServiceBus.CleanUpDeadLettersAsync.LeaveDeadlLetterInQueue");

				await Task.CompletedTask;
			};
			Func<ServiceBusReceiver, ServiceBusReceivedMessage, Task> removeDeadlLetterFromQueue = async (receiver, deadLetterBrokeredMessage) =>
			{
				// Remove message from queue
				try
				{
					await receiver.CompleteMessageAsync(deadLetterBrokeredMessage);
					lockIssues = 0;
				}
				catch (ServiceBusException ex)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for complete for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId), "Cqrs.Azure.Functions.ServiceBus.CleanUpDeadLettersAsync.RemoveDeadlLetterFromQueue", exception: ex);
				}
				Logger.LogDebug(string.Format("A dead-letter message arrived with the id '{0}' but was removed as processing was skipped due to settings.", deadLetterBrokeredMessage.MessageId), "Cqrs.Azure.Functions.ServiceBus.CleanUpDeadLettersAsync.RemoveDeadlLetterFromQueue");

				await Task.CompletedTask;
			};

			await Task.Factory.StartNewSafely(async () =>
			{
				int loop = 0;
				while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
				{
					lockIssues = 0;
					IEnumerable<ServiceBusReceivedMessage> brokeredMessages;

					ServiceBusReceiver deadLetterReceiver = client.CreateReceiver(topicName, topicSubscriptionName, new ServiceBusReceiverOptions
					{
						ReceiveMode = ServiceBusReceiveMode.PeekLock,
						SubQueue = SubQueue.DeadLetter
					});

					brokeredMessages = await deadLetterReceiver.ReceiveMessagesAsync(1000);

					foreach (ServiceBusReceivedMessage brokeredMessage in brokeredMessages)
					{
						if (lockIssues > 10)
							break;
						try
						{
							Logger.LogDebug($"A dead-letter message arrived with the id '{brokeredMessage.MessageId}'.");
							string messageBody = brokeredMessage.GetBodyAsString();

							// Closure protection
							ServiceBusReceivedMessage message = brokeredMessage;
							try
							{
								AzureBusHelper.ReceiveEvent
								(
									null,
									messageBody,
									@event =>
									{
										bool isRequired = BusHelper.IsEventRequired(@event.GetType());
										if (!isRequired)
										{
											Task.Run(async () => {
												await removeDeadlLetterFromQueue(deadLetterReceiver, message);
											}).Wait();
										}
										else
										{
											Task.Run(async () => {
												await leaveDeadlLetterInQueue(deadLetterReceiver, message, @event);
											}).Wait();
										}
										return true;
									},
									$"id '{brokeredMessage.MessageId}'",
									ExtractSignature(message),
									SigningTokenConfigurationKey,
									() =>
									{
										Task.Run(async () => {
											await removeDeadlLetterFromQueue(deadLetterReceiver, message);
										}).Wait();
									},
									() => { }
								);
							}
							catch
							{
								AzureBusHelper.ReceiveCommand
								(
									null,
									messageBody,
									command =>
									{
										bool isRequired = BusHelper.IsEventRequired(command.GetType());
										if (!isRequired)
										{
											Task.Run(async () => {
												await removeDeadlLetterFromQueue(deadLetterReceiver, message);
											}).Wait();
										}
										else
										{
											Task.Run(async () => {
												await leaveDeadlLetterInQueue(deadLetterReceiver, message, command);
											}).Wait();
										}
										return true;
									},
									$"id '{brokeredMessage.MessageId}'",
									ExtractSignature(message),
									SigningTokenConfigurationKey,
									() =>
									{
										Task.Run(async () => {
											await removeDeadlLetterFromQueue(deadLetterReceiver, message);
										}).Wait();
									},
									() => { }
								);
							}
						}
						catch (Exception exception)
						{
							TelemetryHelper.TrackException(exception, null, telemetryProperties);
							// Indicates a problem, unlock message in queue
							Logger.LogError($"A dead-letter message arrived with the id '{brokeredMessage.MessageId}' but failed to be process.", exception: exception);
							try
							{
								await deadLetterReceiver.AbandonMessageAsync(brokeredMessage);
							}
							catch (ServiceBusException)
							{
								lockIssues++;
								Logger.LogWarning($"The lock supplied for abandon for the skipped dead-letter message '{brokeredMessage.MessageId}' is invalid.");
							}
						}
					}

					if (loop++ % 5 == 0)
					{
						loop = 0;
						Thread.Yield();
					}
					else
						Thread.Sleep(500);
				}
				try
				{
					brokeredMessageRenewCancellationTokenSource.Dispose();
				}
				catch (ObjectDisposedException) { }

				return Task.CompletedTask;
			}, brokeredMessageRenewCancellationTokenSource.Token);

			return brokeredMessageRenewCancellationTokenSource;
		}

		/// <summary>
		/// Replaced with <see cref="ExtractTelemetryProperties(ServiceBusReceivedMessage, string)"/>
		/// </summary>
		[Obsolete("Replaced with ExtractTelemetryProperties.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override IDictionary<string, string> ExtractTelemetryProperties
		(
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
			Microsoft.Azure.ServiceBus.Message
#else
			Microsoft.ServiceBus.Messaging.BrokeredMessage
#endif
			message,
			string baseCommunicationType
		)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with ExtractTelemetryProperties.");
		}
		/// <summary>
		/// Extract any telemetry properties from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual IDictionary<string, string> ExtractTelemetryProperties(ServiceBusReceivedMessage message, string baseCommunicationType)
		{
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", baseCommunicationType } };
			object value;
			if (message.TryGetUserPropertyValue("Type", out value))
				telemetryProperties.Add("MessageType", value.ToString());
			if (message.TryGetUserPropertyValue("Source", out value))
				telemetryProperties.Add("MessageSource", value.ToString());
			if (message.TryGetUserPropertyValue("Source-Method", out value))
				telemetryProperties.Add("MessageSourceMethod", value.ToString());
			if (message.TryGetUserPropertyValue("CorrelationId", out value) && !telemetryProperties.ContainsKey("CorrelationId"))
				telemetryProperties.Add("CorrelationId", value.ToString());

			return telemetryProperties;
		}

		/// <summary>
		/// Replaced with <see cref="ExtractSignature(ServiceBusReceivedMessage)"/>
		/// </summary>
		[Obsolete("Replaced with ExtractSignature.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override string ExtractSignature
		(
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
			Microsoft.Azure.ServiceBus.Message
#else
			Microsoft.ServiceBus.Messaging.BrokeredMessage
#endif
			message
		)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with ExtractSignature.");
		}
		/// <summary>
		/// Extract the signature from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual string ExtractSignature(ServiceBusReceivedMessage message)
		{
			object value;
			if (message.TryGetUserPropertyValue("Signature", out value))
				return value.ToString();
			return null;
		}

		/// <summary>
		/// Triggers settings checking on both public and private publishers and receivers,
		/// then calls <see cref="AzureServiceBus{TAuthenticationToken}.InstantiatePublishing"/> if <see cref="AzureServiceBus{TAuthenticationToken}.PublicServiceBusPublisher"/> is not null.
		/// </summary>
		protected override void TriggerSettingsChecking()
		{
			// First refresh the EventBlackListProcessing property
			bool throwExceptionOnReceiverMessageLockLostExceptionDuringComplete;
			if (!ConfigurationManager.TryGetSetting(ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey, out throwExceptionOnReceiverMessageLockLostExceptionDuringComplete))
				throwExceptionOnReceiverMessageLockLostExceptionDuringComplete = true;
			ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete = throwExceptionOnReceiverMessageLockLostExceptionDuringComplete;

			Task.Run(async () => {
				await TriggerSettingsCheckingAsync(PrivateServiceBusPublisher, PrivateServiceBusReceivers);
				await TriggerSettingsCheckingAsync(PublicServiceBusPublisher, PublicServiceBusReceivers);
			}).Wait();

			// Restart configuration, we order this intentionally with the publisher second as if this triggers the cancellation there's nothing else to process here
			// we also only need to check one of the publishers
			if (PublicServiceBusPublisher != null)
			{
				Logger.LogDebug("Recursively calling into InstantiatePublishing.");
				InstantiatePublishing();
			}
		}

		/// <summary>
		/// Replaced with <see cref="TriggerSettingsCheckingAsync(ServiceBusSender, IDictionary{int, ServiceBusProcessor})"/>
		/// </summary>
		[Obsolete("Replaced with TriggerSettingsCheckingAsync.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void TriggerSettingsChecking(OldTopicClient serviceBusPublisher, IDictionary<int, OldIMessageReceiver> serviceBusReceivers)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with TriggerSettingsCheckingAsync.");
		}
		/// <summary>
		/// Triggers settings checking on the provided <paramref name="serviceBusPublisher"/> and <paramref name="serviceBusReceivers"/>,
		/// then calls <see cref="InstantiateReceiving()"/>.
		/// </summary>
		protected virtual async Task TriggerSettingsCheckingAsync(ServiceBusSender serviceBusPublisher, IDictionary<int, ServiceBusProcessor> serviceBusReceivers)
		{
			// Let's wrap up using this message bus and start the switch
			if (serviceBusPublisher != null)
			{
				await serviceBusPublisher.CloseAsync();
				Logger.LogDebug("Publishing service bus closed.");
			}
			foreach (ServiceBusProcessor serviceBusReceiver in serviceBusReceivers.Values)
			{
				// Let's wrap up using this message bus and start the switch
				if (serviceBusReceiver != null)
				{
					await serviceBusReceiver.CloseAsync();
					await serviceBusReceiver.DisposeAsync();
					Logger.LogDebug("Receiving service bus closed.");
				}
				// Restart configuration, we order this intentionally with the receiver first as if this triggers the cancellation we know this isn't a publisher as well
				if (serviceBusReceiver != null)
				{
					Logger.LogDebug("Recursively calling into InstantiateReceiving.");
					InstantiateReceiving();

					// This will be the case of a connection setting change re-connection
					if (ReceiverMessageHandler != null && ReceiverMessageHandlerOptions != null)
					{
						// Callback to handle received messages
						Logger.LogDebug("Re-registering onMessage handler.");
						ApplyReceiverMessageHandler();
					}
					else
						Logger.LogWarning("No onMessage handler was found to re-bind.");
				}
			}

			await Task.CompletedTask;
		}

		/// <summary>
		/// Applies the stored ReceiverMessageHandler and ReceiverMessageHandlerOptions to all receivers in
		/// <see cref="PrivateServiceBusReceivers"/> and <see cref="PublicServiceBusReceivers"/>.
		/// </summary>
		protected override void ApplyReceiverMessageHandler()
		{
			foreach (ServiceBusProcessor processor in PrivateServiceBusReceivers.Values)
			{
				processor.ProcessMessageAsync += async args => {
					BusHelper.SetWasPrivateBusUsed(true);
					await FunctionReceiverMessageHandler(args);
				};
			}
			foreach (ServiceBusProcessor processor in PublicServiceBusReceivers.Values)
			{
				processor.ProcessMessageAsync += async args => {
					BusHelper.SetWasPrivateBusUsed(false);
					await FunctionReceiverMessageHandler(args);
				};
			}
		}

		#endregion
	}
}