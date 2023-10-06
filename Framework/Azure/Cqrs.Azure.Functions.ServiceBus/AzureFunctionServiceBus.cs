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
#else
using Microsoft.ServiceBus.Messaging;
using OldManager = Microsoft.ServiceBus.NamespaceManager;
using OldIMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
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
		/// Gets the private <see cref="ServiceBusReceiver"/> receivers.
		/// </summary>
		protected IDictionary<int, ServiceBusReceiver> PrivateServiceBusReceivers { get; private set; }

		/// <summary>
		/// Gets the public <see cref="ServiceBusReceiver"/> receivers.
		/// </summary>
		protected IDictionary<int, ServiceBusReceiver> PublicServiceBusReceivers { get; private set; }

		/// <summary>
		/// The <see cref="Action{IMessageReceiver, TBrokeredMessage}">handler</see> used for <see cref="MessageReceiver.RegisterMessageHandler(Func{BrokeredMessage, CancellationToken, Task}, MessageHandlerOptions)"/> on each receiver.
		/// </summary>
		protected virtual Action<ServiceBusReceiver, ServiceBusReceivedMessage> ReceiverMessageHandler { get; set; }

		/// <summary>
		/// The <see cref="ServiceBusProcessorOptions" /> used.
		/// </summary>
		protected virtual ServiceBusProcessorOptions ReceiverMessageHandlerOptions { get; set; }

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
		/// Creates a single <see cref="ServiceBusReceiver"/>.
		/// </summary>
		/// <param name="client">The <see cref="ServiceBusClient"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="ServiceBusReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
		protected virtual async Task InstantiateReceivingAsync(ServiceBusClient client, IDictionary<int, ServiceBusReceiver> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			ServiceBusReceiver serviceBusReceiver = client.CreateReceiver(topicName, topicSubscriptionName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, Identifier = Logger.LoggerSettings.ModuleName });

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
		/// Checks if the private topic and subscription name exists as per <see cref="PrivateTopicName"/> and <see cref="PrivateTopicSubscriptionName"/>.
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
		/// Checks if the public topic and subscription name exists as per <see cref="PublicTopicName"/> and <see cref="PublicTopicSubscriptionName"/>.
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
		protected async virtual Task RegisterReceiverMessageHandlerAsync(Action<ServiceBusReceiver, ServiceBusReceivedMessage> receiverMessageHandler, ServiceBusProcessorOptions receiverMessageHandlerOptions)
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
		protected virtual void StoreReceiverMessageHandler(Action<Microsoft.Azure.ServiceBus.Core.IMessageReceiver, Microsoft.Azure.ServiceBus.Message> receiverMessageHandler, Microsoft.Azure.ServiceBus.MessageHandlerOptions receiverMessageHandlerOptions)
#else
		protected virtual void StoreReceiverMessageHandler(Action<Microsoft.ServiceBus.Messaging.SubscriptionClient, Microsoft.ServiceBus.Messaging.BrokeredMessage> receiverMessageHandler, Microsoft.ServiceBus.Messaging.OnMessageOptions receiverMessageHandlerOptions)
#endif
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			throw new NotImplementedException("Replaced with StoreReceiverMessageHandlerAsync.");
		}
		/// <summary>
		/// Stores the provided <paramref name="receiverMessageHandler"/> and <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
		protected async virtual Task StoreReceiverMessageHandlerAsync(Action<ServiceBusReceiver, ServiceBusReceivedMessage> receiverMessageHandler, ServiceBusProcessorOptions receiverMessageHandlerOptions)
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;

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

		#endregion
	}
}