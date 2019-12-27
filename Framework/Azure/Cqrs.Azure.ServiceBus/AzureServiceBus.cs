#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;
#if NET452
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif
#if NETSTANDARD2_0
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// An <see cref="AzureBus{TAuthenticationToken}"/> that uses Azure Service Bus.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <remarks>
	/// https://markheath.net/post/migrating-to-new-servicebus-sdk
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#receive-messages-from-the-subscription
	/// https://stackoverflow.com/questions/47427361/azure-service-bus-read-messages-sent-by-net-core-2-with-brokeredmessage-getbo
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
	/// </remarks>
	public abstract class AzureServiceBus<TAuthenticationToken>
		: AzureBus<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the private <see cref="TopicClient"/> publisher.
		/// </summary>
		protected TopicClient PrivateServiceBusPublisher { get; private set; }

		/// <summary>
		/// Gets the public <see cref="TopicClient"/> publisher.
		/// </summary>
		protected TopicClient PublicServiceBusPublisher { get; private set; }

		/// <summary>
		/// Gets the private <see cref="IMessageReceiver"/> receivers.
		/// </summary>
		protected IDictionary<int, IMessageReceiver> PrivateServiceBusReceivers { get; private set; }

		/// <summary>
		/// Gets the public <see cref="IMessageReceiver"/> receivers.
		/// </summary>
		protected IDictionary<int, IMessageReceiver> PublicServiceBusReceivers { get; private set; }

		/// <summary>
		/// The name of the private topic.
		/// </summary>
		protected string PrivateTopicName { get; private set; }

		/// <summary>
		/// The name of the public topic.
		/// </summary>
		protected string PublicTopicName { get; private set; }

		/// <summary>
		/// The name of the subscription in the private topic.
		/// </summary>
		protected string PrivateTopicSubscriptionName { get; private set; }

		/// <summary>
		/// The name of the subscription in the public topic.
		/// </summary>
		protected string PublicTopicSubscriptionName { get; private set; }

		/// <summary>
		/// The configuration key for the message bus connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string MessageBusConnectionStringConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string SigningTokenConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PrivateTopicNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PublicTopicNameConfigurationKey { get; }

		/// <summary>
		/// The default name of the private topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected abstract string DefaultPrivateTopicName { get; }

		/// <summary>
		/// The default name of the public topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected abstract string DefaultPublicTopicName { get; }

		/// <summary>
		/// The configuration key for the name of the subscription in the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PrivateTopicSubscriptionNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the subscription in the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PublicTopicSubscriptionNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key that
		/// specifies if an <see cref="Exception"/> is thrown if the network lock is lost
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey { get; }

		/// <summary>
		/// Specifies if an <see cref="Exception"/> is thrown if the network lock is lost.
		/// </summary>
		protected bool ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete { get; private set; }

		/// <summary>
		/// The default name of the subscription in the private topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected const string DefaultPrivateTopicSubscriptionName = "Root";

		/// <summary>
		/// The default name of the subscription in the public topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected const string DefaultPublicTopicSubscriptionName = "Root";

#if NET452
		/// <summary>
		/// The <see cref="Action{TBrokeredMessage}">handler</see> used for <see cref="IMessageReceiver.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage}, OnMessageOptions)"/> on each receiver.
		/// </summary>
		protected Action<BrokeredMessage> ReceiverMessageHandler { get; set; }
#endif
#if NETSTANDARD2_0
		/// <summary>
		/// The <see cref="Action{IMessageReceiver, TBrokeredMessage}">handler</see> used for <see cref="MessageReceiver.RegisterMessageHandler(Func{BrokeredMessage, CancellationToken, Task}, MessageHandlerOptions)"/> on each receiver.
		/// </summary>
		protected Action<IMessageReceiver, BrokeredMessage> ReceiverMessageHandler { get; set; }
#endif

#if NET452
		/// <summary>
		/// The <see cref="OnMessageOptions" /> used for <see cref="IMessageReceiver.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage}, OnMessageOptions)"/> on each receiver.
		/// </summary>
		protected OnMessageOptions ReceiverMessageHandlerOptions { get; set; }
#endif
#if NETSTANDARD2_0
		/// <summary>
		/// The <see cref="MessageHandlerOptions" /> used for <see cref="MessageReceiver.RegisterMessageHandler(Func{BrokeredMessage, CancellationToken, Task}, MessageHandlerOptions)"/> on each receiver.
		/// </summary>
		protected MessageHandlerOptions ReceiverMessageHandlerOptions { get; set; }
#endif

		/// <summary>
		/// Gets the <see cref="IBusHelper"/>.
		/// </summary>
		protected IBusHelper BusHelper { get; private set; }

		/// <summary>
		/// Gets the <see cref="IAzureBusHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }

		/// <summary>
		/// Gets the <see cref="ITelemetryHelper"/>.
		/// </summary>
		protected ITelemetryHelper TelemetryHelper { get; set; }

		/// <summary>
		/// The maximum number of time a retry is tried if a <see cref="System.TimeoutException"/> is thrown while sending messages.
		/// </summary>
		protected short TimeoutOnSendRetryMaximumCount { get; private set; }

		/// <summary>
		/// The <see cref="IHashAlgorithmFactory"/> to use to sign messages.
		/// </summary>
		protected IHashAlgorithmFactory Signer { get; private set; }

		/// <summary>
		/// A list of namespaces to exclude when trying to automatically determine the container.
		/// </summary>
		protected IList<string> ExclusionNamespaces { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureServiceBus{TAuthenticationToken}"/>
		/// </summary>
		protected AzureServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
			BusHelper = busHelper;
			TelemetryHelper = new NullTelemetryHelper();
			PrivateServiceBusReceivers = new Dictionary<int, IMessageReceiver>();
			PublicServiceBusReceivers = new Dictionary<int, IMessageReceiver>();
			TimeoutOnSendRetryMaximumCount = 1;
			string timeoutOnSendRetryMaximumCountValue;
			short timeoutOnSendRetryMaximumCount;
			if (ConfigurationManager.TryGetSetting("Cqrs.Azure.Servicebus.TimeoutOnSendRetryMaximumCount", out timeoutOnSendRetryMaximumCountValue) && !string.IsNullOrWhiteSpace(timeoutOnSendRetryMaximumCountValue) && short.TryParse(timeoutOnSendRetryMaximumCountValue, out timeoutOnSendRetryMaximumCount))
				TimeoutOnSendRetryMaximumCount = timeoutOnSendRetryMaximumCount;
			ExclusionNamespaces = new SynchronizedCollection<string> { "Cqrs", "System" };
			Signer = hashAlgorithmFactory;
		}

		#region Overrides of AzureBus<TAuthenticationToken>

		/// <summary>
		/// Gets the connection string for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected override string GetConnectionString()
		{
			string connectionString = ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ConfigurationErrorsException(string.Format("Configuration is missing required information. Make sure the appSetting '{0}' is defined and has a valid connection string value.", MessageBusConnectionStringConfigurationKey));
			return connectionString;
		}

		#endregion

		/// <summary>
		/// Instantiate publishing on this bus by
		/// calling <see cref="CheckPrivateTopicExists"/> and <see cref="CheckPublicTopicExists"/>
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiatePublishing()
		{
#if NET452
			Manager manager = Manager.CreateFromConnectionString(ConnectionString);
#endif
#if NETSTANDARD2_0
			var manager = new Manager(ConnectionString);
#endif
			CheckPrivateTopicExists(manager);
			CheckPublicTopicExists(manager);

#if NET452
			PrivateServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PrivateTopicName);
			PublicServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
#endif
#if NETSTANDARD2_0
			PrivateServiceBusPublisher = new TopicClient(ConnectionString, PrivateTopicName);
			PublicServiceBusPublisher = new TopicClient(ConnectionString, PublicTopicName);
#endif
			StartSettingsChecking();
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
#if NET452
			Manager manager = Manager.CreateFromConnectionString(ConnectionString);
#endif
#if NETSTANDARD2_0
			var manager = new Manager(ConnectionString);
#endif

			CheckPrivateTopicExists(manager);
			CheckPublicTopicExists(manager);

			try
			{
				InstantiateReceiving(manager, PrivateServiceBusReceivers, PrivateTopicName, PrivateTopicSubscriptionName);
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the private Service Bus receivers may be invalid.", exception);
			}
			try
			{
				InstantiateReceiving(manager, PublicServiceBusReceivers, PublicTopicName, PublicTopicSubscriptionName);
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the public Service Bus receivers may be invalid.", exception);
			}

			bool enableDeadLetterCleanUp;
			string enableDeadLetterCleanUpValue = ConfigurationManager.GetSetting("Cqrs.Azure.Servicebus.EnableDeadLetterCleanUp");
			if (bool.TryParse(enableDeadLetterCleanUpValue, out enableDeadLetterCleanUp) && enableDeadLetterCleanUp)
			{
				CleanUpDeadLetters(PrivateTopicName, PrivateTopicSubscriptionName);
				CleanUpDeadLetters(PublicTopicName, PublicTopicSubscriptionName);
			}

			// If this is also a publisher, then it will the check over there and that will handle this
			// we only need to check one of these
			if (PublicServiceBusPublisher != null)
				return;

			StartSettingsChecking();
		}

#if NET452
		/// <summary>
		/// Creates <see cref="AzureBus{TAuthenticationToken}.NumberOfReceiversCount"/> <see cref="IMessageReceiver"/>.
		/// If flushing is required, any flushed <see cref="IMessageReceiver"/> has <see cref="ClientEntity.Close()"/> called on it first.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
#endif
#if NETSTANDARD2_0
		/// <summary>
		/// Creates <see cref="AzureBus{TAuthenticationToken}.NumberOfReceiversCount"/> <see cref="IMessageReceiver"/>.
		/// If flushing is required, any flushed <see cref="IMessageReceiver"/> has <see cref="ClientEntity.CloseAsync()"/> called on it first.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
#endif
		protected virtual void InstantiateReceiving(Manager manager, IDictionary<int, IMessageReceiver> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			for (int i = 0; i < NumberOfReceiversCount; i++)
			{
#if NET452
				IMessageReceiver serviceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicName, topicSubscriptionName);
#endif
#if NETSTANDARD2_0
				IMessageReceiver serviceBusReceiver = new MessageReceiver(ConnectionString, EntityNameHelper.FormatSubscriptionPath(topicName, topicSubscriptionName));
#endif
				if (serviceBusReceivers.ContainsKey(i))
					serviceBusReceivers[i] = serviceBusReceiver;
				else
					serviceBusReceivers.Add(i, serviceBusReceiver);
			}
			// Remove any if the number has decreased
			for (int i = NumberOfReceiversCount; i < serviceBusReceivers.Count; i++)
			{
				IMessageReceiver serviceBusReceiver;
				if (serviceBusReceivers.TryGetValue(i, out serviceBusReceiver))
				{
#if NET452
					serviceBusReceiver.Close();
#endif
#if NETSTANDARD2_0
					serviceBusReceiver.CloseAsync().Wait(1500);
#endif
				}
				serviceBusReceivers.Remove(i);
			}
		}

		/// <summary>
		/// Checks if the private topic and subscription name exists as per <see cref="PrivateTopicName"/> and <see cref="PrivateTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		protected virtual void CheckPrivateTopicExists(Manager manager)
		{
			CheckTopicExists(manager, PrivateTopicName = ConfigurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName, PrivateTopicSubscriptionName = ConfigurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName);
		}

		/// <summary>
		/// Checks if the public topic and subscription name exists as per <see cref="PublicTopicName"/> and <see cref="PublicTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		protected virtual void CheckPublicTopicExists(Manager manager)
		{
			CheckTopicExists(manager, PublicTopicName = ConfigurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName, PublicTopicSubscriptionName = ConfigurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName);
		}

		/// <summary>
		/// Checks if a topic by the provided <paramref name="topicName"/> exists and
		/// Checks if a subscription name by the provided <paramref name="subscriptionName"/> exists.
		/// </summary>
		protected virtual void CheckTopicExists(Manager manager, string topicName, string subscriptionName)
		{
			// Configure Queue Settings
			var eventTopicDescription = new TopicDescription(topicName)
			{
#if NET452
				MaxSizeInMegabytes = 5120,
#endif
#if NETSTANDARD2_0
				MaxSizeInMB = 5120,
#endif
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true,
			};

#if NETSTANDARD2_0
			Task<bool> checkTask = manager.TopicExistsAsync(topicName);
			checkTask.Wait(1500);
			if (!checkTask.Result)
			{
				Task<TopicDescription> createTopicTask = manager.CreateTopicAsync(eventTopicDescription);
				createTopicTask.Wait(1500);
			}

			checkTask = manager.SubscriptionExistsAsync(topicName, subscriptionName);
			checkTask.Wait(1500);
			if (!checkTask.Result)
			{
				var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName)
				{
					DefaultMessageTimeToLive = eventTopicDescription.DefaultMessageTimeToLive,
					EnableBatchedOperations = eventTopicDescription.EnableBatchedOperations,
				};
				Task<SubscriptionDescription> createTask = manager.CreateSubscriptionAsync(subscriptionDescription);
				createTask.Wait(1500);
			}
#endif

#if NET452
			// Create the topic if it does not exist already
			if (!manager.TopicExists(eventTopicDescription.Path))
				manager.CreateTopic(eventTopicDescription);

			if (!manager.SubscriptionExists(eventTopicDescription.Path, subscriptionName))
				manager.CreateSubscription
				(
					new SubscriptionDescription(eventTopicDescription.Path, subscriptionName)
					{
						DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
						EnableBatchedOperations = true,
						EnableDeadLetteringOnFilterEvaluationExceptions = true
					}
				);
#endif
		}

		/// <summary>
		/// Triggers settings checking on both public and private publishers and receivers,
		/// then calls <see cref="InstantiatePublishing"/> if <see cref="PublicServiceBusPublisher"/> is not null.
		/// </summary>
		protected override void TriggerSettingsChecking()
		{
			// First refresh the EventBlackListProcessing property
			bool throwExceptionOnReceiverMessageLockLostExceptionDuringComplete;
			if (!ConfigurationManager.TryGetSetting(ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey, out throwExceptionOnReceiverMessageLockLostExceptionDuringComplete))
				throwExceptionOnReceiverMessageLockLostExceptionDuringComplete = true;
			ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete = throwExceptionOnReceiverMessageLockLostExceptionDuringComplete;

			TriggerSettingsChecking(PrivateServiceBusPublisher, PrivateServiceBusReceivers);
			TriggerSettingsChecking(PublicServiceBusPublisher, PublicServiceBusReceivers);

			// Restart configuration, we order this intentionally with the publisher second as if this triggers the cancellation there's nothing else to process here
			// we also only need to check one of the publishers
			if (PublicServiceBusPublisher != null)
			{
				Logger.LogDebug("Recursively calling into InstantiatePublishing.");
				InstantiatePublishing();
			}
		}

		/// <summary>
		/// Triggers settings checking on the provided <paramref name="serviceBusPublisher"/> and <paramref name="serviceBusReceivers"/>,
		/// then calls <see cref="InstantiateReceiving()"/>.
		/// </summary>
		protected virtual void TriggerSettingsChecking(TopicClient serviceBusPublisher, IDictionary<int, IMessageReceiver> serviceBusReceivers)
		{
			// Let's wrap up using this message bus and start the switch
			if (serviceBusPublisher != null)
			{
#if NET452
				serviceBusPublisher.Close();
#endif
#if NETSTANDARD2_0
				serviceBusPublisher.CloseAsync().Wait(1500);
#endif
				Logger.LogDebug("Publishing service bus closed.");
			}
			foreach (IMessageReceiver serviceBusReceiver in serviceBusReceivers.Values)
			{
				// Let's wrap up using this message bus and start the switch
				if (serviceBusReceiver != null)
				{
#if NET452
					serviceBusReceiver.Close();
#endif
#if NETSTANDARD2_0
					serviceBusReceiver.CloseAsync().Wait(1500);
#endif
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
		}

		/// <summary>
		/// Registers the provided <paramref name="receiverMessageHandler"/> with the provided <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
#if NET452
		protected virtual void RegisterReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
#endif
#if NETSTANDARD2_0
		protected virtual void RegisterReceiverMessageHandler(Action<IMessageReceiver, BrokeredMessage> receiverMessageHandler, MessageHandlerOptions receiverMessageHandlerOptions)
#endif
		{
			StoreReceiverMessageHandler(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();
		}

		/// <summary>
		/// Stores the provided <paramref name="receiverMessageHandler"/> and <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
#if NET452
		protected virtual void StoreReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
#endif
#if NETSTANDARD2_0
		protected virtual void StoreReceiverMessageHandler(Action<IMessageReceiver, BrokeredMessage> receiverMessageHandler, MessageHandlerOptions receiverMessageHandlerOptions)
#endif
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;
		}

		/// <summary>
		/// Applies the stored ReceiverMessageHandler and ReceiverMessageHandlerOptions to all receivers in
		/// <see cref="PrivateServiceBusReceivers"/> and <see cref="PublicServiceBusReceivers"/>.
		/// </summary>
		protected override void ApplyReceiverMessageHandler()
		{
			foreach (IMessageReceiver serviceBusReceiver in PrivateServiceBusReceivers.Values)
			{
#if NET452
				serviceBusReceiver
					.OnMessage
					(
						message =>
						{
							BusHelper.SetWasPrivateBusUsed(true);
							ReceiverMessageHandler(message);
						},
						ReceiverMessageHandlerOptions
					);
#endif
#if NETSTANDARD2_0
				serviceBusReceiver
					.RegisterMessageHandler
					(
						(message, cancellationToken) =>
						{
							return Task.Factory.StartNewSafely(() => {
								BusHelper.SetWasPrivateBusUsed(true);
								ReceiverMessageHandler(serviceBusReceiver, message);
							});
						},
						ReceiverMessageHandlerOptions
					);
#endif
			}
			foreach (IMessageReceiver serviceBusReceiver in PublicServiceBusReceivers.Values)
			{
#if NET452
				serviceBusReceiver
					.OnMessage
						(
							message =>
							{
								BusHelper.SetWasPrivateBusUsed(false);
								ReceiverMessageHandler(message);
							},
							ReceiverMessageHandlerOptions
						);
#endif
#if NETSTANDARD2_0
				serviceBusReceiver
					.RegisterMessageHandler
					(
						(message, cancellationToken) =>
						{
							return Task.Factory.StartNewSafely(() => {
								BusHelper.SetWasPrivateBusUsed(false);
								ReceiverMessageHandler(serviceBusReceiver, message);
							});
						},
						ReceiverMessageHandlerOptions
					);
#endif
			}
		}

		/// <summary>
		/// Using a <see cref="Task"/>, clears all dead letters from the topic and subscription of the 
		/// provided <paramref name="topicName"/> and <paramref name="topicSubscriptionName"/>.
		/// </summary>
		/// <param name="topicName">The name of the topic.</param>
		/// <param name="topicSubscriptionName">The name of the subscription.</param>
		/// <returns></returns>
		protected virtual CancellationTokenSource CleanUpDeadLetters(string topicName, string topicSubscriptionName)
		{
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			int lockIssues = 0;

#if NET452
			Action<BrokeredMessage, IMessage> leaveDeadlLetterInQueue = (deadLetterBrokeredMessage, deadLetterMessage) =>
#endif
#if NETSTANDARD2_0
			Action<IMessageReceiver, BrokeredMessage, IMessage> leaveDeadlLetterInQueue = (client, deadLetterBrokeredMessage, deadLetterMessage) =>
#endif
			{
				// Remove message from queue
				try
				{
#if NET452
					deadLetterBrokeredMessage.Abandon();
#endif
#if NETSTANDARD2_0
					client.AbandonAsync(deadLetterBrokeredMessage.SystemProperties.LockToken).Wait(1500);
#endif
					lockIssues = 0;
				}
				catch (MessageLockLostException)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId));
				}
				Logger.LogDebug(string.Format("A dead-letter message of type {0} arrived with the id '{1}' but left in the queue due to settings.", deadLetterMessage.GetType().FullName, deadLetterBrokeredMessage.MessageId));
			};
#if NET452
			Action <BrokeredMessage> removeDeadlLetterFromQueue = (deadLetterBrokeredMessage) =>
#endif
#if NETSTANDARD2_0
			Action<IMessageReceiver, BrokeredMessage> removeDeadlLetterFromQueue = (client, deadLetterBrokeredMessage) =>
#endif
			{
				// Remove message from queue
				try
				{
#if NET452
					deadLetterBrokeredMessage.Complete();
#endif
#if NETSTANDARD2_0
					client.CompleteAsync(deadLetterBrokeredMessage.SystemProperties.LockToken).Wait(1500);
#endif
					lockIssues = 0;
				}
				catch (MessageLockLostException)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for complete for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId));
				}
				Logger.LogDebug(string.Format("A dead-letter message arrived with the id '{0}' but was removed as processing was skipped due to settings.", deadLetterBrokeredMessage.MessageId));
			};

			Task.Factory.StartNewSafely(() =>
			{
				int loop = 0;
				while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
				{
					lockIssues = 0;
					IEnumerable<BrokeredMessage> brokeredMessages;

#if NET452
					MessagingFactory factory = MessagingFactory.CreateFromConnectionString(ConnectionString);
					string deadLetterPath = SubscriptionClient.FormatDeadLetterPath(topicName, topicSubscriptionName);
					MessageReceiver client = factory.CreateMessageReceiver(deadLetterPath, ReceiveMode.PeekLock);
					brokeredMessages = client.ReceiveBatch(1000);
#endif
#if NETSTANDARD2_0
					string deadLetterPath = EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topicName, topicSubscriptionName));
					MessageReceiver client = new MessageReceiver(ConnectionString, deadLetterPath, ReceiveMode.PeekLock);
					Task<IList<BrokeredMessage>> receiveTask = client.ReceiveAsync(1000);
					receiveTask.Wait(10000);
					if (receiveTask.IsCompleted && receiveTask.Result != null)
						brokeredMessages = receiveTask.Result;
					else
						brokeredMessages = Enumerable.Empty<BrokeredMessage>();
#endif

					foreach (BrokeredMessage brokeredMessage in brokeredMessages)
					{
						if (lockIssues > 10)
							break;
						try
						{
							Logger.LogDebug(string.Format("A dead-letter message arrived with the id '{0}'.", brokeredMessage.MessageId));
							string messageBody = brokeredMessage.GetBodyAsString();

							// Closure protection
							BrokeredMessage message = brokeredMessage;
							try
							{
								AzureBusHelper.ReceiveEvent
								(
									messageBody,
									@event =>
									{
										bool isRequired = BusHelper.IsEventRequired(@event.GetType());
										if (!isRequired)
										{
#if NET452
											removeDeadlLetterFromQueue(message);
#endif
#if NETSTANDARD2_0
											removeDeadlLetterFromQueue(client, message);
#endif
										}
										else
										{
#if NET452
											leaveDeadlLetterInQueue(message, @event);
#endif
#if NETSTANDARD2_0
											leaveDeadlLetterInQueue(client, message, @event);
#endif
										}
										return true;
									},
									string.Format("id '{0}'", brokeredMessage.MessageId),
									ExtractSignature(message),
									SigningTokenConfigurationKey,
									() =>
									{
#if NET452
										removeDeadlLetterFromQueue(message);
#endif
#if NETSTANDARD2_0
										removeDeadlLetterFromQueue(client, message);
#endif
									},
									() => { }
								);
							}
							catch
							{
								AzureBusHelper.ReceiveCommand
								(
									messageBody,
									command =>
									{
										bool isRequired = BusHelper.IsEventRequired(command.GetType());
										if (!isRequired)
										{
#if NET452
											removeDeadlLetterFromQueue(message);
#endif
#if NETSTANDARD2_0
											removeDeadlLetterFromQueue(client, message);
#endif
										}
										else
										{
#if NET452
											leaveDeadlLetterInQueue(message, command);
#endif
#if NETSTANDARD2_0
											leaveDeadlLetterInQueue(client, message, command);
#endif
										}
										return true;
									},
									string.Format("id '{0}'", brokeredMessage.MessageId),
									ExtractSignature(message),
									SigningTokenConfigurationKey,
									() =>
									{
#if NET452
										removeDeadlLetterFromQueue(message);
#endif
#if NETSTANDARD2_0
										removeDeadlLetterFromQueue(client, message);
#endif
									},
									() => { }
								);
							}
						}
						catch (Exception exception)
						{
							TelemetryHelper.TrackException(exception, null, telemetryProperties);
							// Indicates a problem, unlock message in queue
							Logger.LogError(string.Format("A dead-letter message arrived with the id '{0}' but failed to be process.", brokeredMessage.MessageId), exception: exception);
							try
							{
#if NET452
								brokeredMessage.Abandon();
#endif
#if NETSTANDARD2_0
								client.AbandonAsync(brokeredMessage.SystemProperties.LockToken).Wait(1500);
#endif
							}
							catch (MessageLockLostException)
							{
								lockIssues++;
								Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", brokeredMessage.MessageId));
							}
						}
					}
#if NET452
					client.Close();
#endif
#if NETSTANDARD2_0
					client.CloseAsync().Wait(1500);
#endif

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
			}, brokeredMessageRenewCancellationTokenSource.Token);

			return brokeredMessageRenewCancellationTokenSource;
		}

#if NETSTANDARD2_0
		DataContractSerializer brokeredMessageSerialiser = new DataContractSerializer(typeof(string));
#endif
		/// <summary>
		/// Create a <see cref="BrokeredMessage"/> with additional properties to aid routing and tracing
		/// </summary>
		protected virtual BrokeredMessage CreateBrokeredMessage<TMessage>(Func<TMessage, string> serialiserFunction, Type messageType, TMessage message)
		{
			string messageBody = serialiserFunction(message);
#if NET452
			var brokeredMessage = new BrokeredMessage(messageBody)
#endif
#if NETSTANDARD2_0
			byte[] messageBodyData;
			using (var stream = new MemoryStream())
			{
				XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
				brokeredMessageSerialiser.WriteObject(binaryDictionaryWriter, messageBody);
				binaryDictionaryWriter.Flush();
				messageBodyData = stream.ToArray();
			}

			var brokeredMessage = new BrokeredMessage(messageBodyData)
#endif
			{
				CorrelationId = CorrelationIdHelper.GetCorrelationId().ToString("N")
			};
			brokeredMessage.AddUserProperty("CorrelationId", brokeredMessage.CorrelationId);
			brokeredMessage.AddUserProperty("Type", messageType.FullName);
			brokeredMessage.AddUserProperty("Source", string.Format("{0}/{1}/{2}/{3}", Logger.LoggerSettings.ModuleName, Logger.LoggerSettings.Instance, Logger.LoggerSettings.Environment, Logger.LoggerSettings.EnvironmentInstance));

			// see https://github.com/Chinchilla-Software-Com/CQRS/wiki/Inter-process-function-security</remarks>
			string configurationKey = string.Format("{0}.SigningToken", messageType.FullName);
			string signingToken;
			HashAlgorithm signer = Signer.Create();
			if (!ConfigurationManager.TryGetSetting(configurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
				if (!ConfigurationManager.TryGetSetting(SigningTokenConfigurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
					signingToken = Guid.Empty.ToString("N");
			if (!string.IsNullOrWhiteSpace(signingToken))
				using (var hashStream = new MemoryStream(Encoding.UTF8.GetBytes($"{signingToken}{messageBody}")))
					brokeredMessage.AddUserProperty("Signature", Convert.ToBase64String(signer.ComputeHash(hashStream)));

			try
			{
				var stackTrace = new StackTrace();
				StackFrame[] stackFrames = stackTrace.GetFrames();
				if (stackFrames != null)
				{
					foreach (StackFrame frame in stackFrames)
					{
						MethodBase method = frame.GetMethod();
						if (method.ReflectedType == null)
							continue;

						try
						{
							if (ExclusionNamespaces.All(@namespace => !method.ReflectedType.FullName.StartsWith(@namespace)))
							{
								brokeredMessage.AddUserProperty("Source-Method", string.Format("{0}.{1}", method.ReflectedType.FullName, method.Name));
								break;
							}
						}
						catch
						{
							// Just move on
						}
					}
				}
			}
			catch
			{
				// Just move on
			}

			return brokeredMessage;
		}

		/// <summary>
		/// Extract any telemetry properties from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual IDictionary<string, string> ExtractTelemetryProperties(BrokeredMessage message, string baseCommunicationType)
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
		/// Extract the signature from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual string ExtractSignature(BrokeredMessage message)
		{
			object value;
			if (message.TryGetUserPropertyValue("Signature", out value))
				return value.ToString();
			return null;
		}
	}
}