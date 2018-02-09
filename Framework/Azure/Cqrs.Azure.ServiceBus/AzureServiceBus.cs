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
using System.Threading;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// An <see cref="AzureBus{TAuthenticationToken}"/> that uses Azure Service Bus.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class AzureServiceBus<TAuthenticationToken> : AzureBus<TAuthenticationToken>
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
		/// Gets the private <see cref="SubscriptionClient"/> receivers.
		/// </summary>
		protected IDictionary<int, SubscriptionClient> PrivateServiceBusReceivers { get; private set; }

		/// <summary>
		/// Gets the public <see cref="SubscriptionClient"/> receivers.
		/// </summary>
		protected IDictionary<int, SubscriptionClient> PublicServiceBusReceivers { get; private set; }

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

		/// <summary>
		/// The <see cref="Action{TBrokeredMessage}">handler</see> used for <see cref="SubscriptionClient.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage}, OnMessageOptions)"/> on each receiver.
		/// </summary>
		protected Action<BrokeredMessage> ReceiverMessageHandler { get; set; }

		/// <summary>
		/// The <see cref="OnMessageOptions" /> used for <see cref="SubscriptionClient.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage}, OnMessageOptions)"/> on each receiver.
		/// </summary>
		protected OnMessageOptions ReceiverMessageHandlerOptions { get; set; }

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
		/// Instantiates a new instance of <see cref="AzureServiceBus{TAuthenticationToken}"/>
		/// </summary>
		protected AzureServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
			BusHelper = busHelper;
			TelemetryHelper = new NullTelemetryHelper();
			PrivateServiceBusReceivers = new Dictionary<int, SubscriptionClient>();
			PublicServiceBusReceivers = new Dictionary<int, SubscriptionClient>();
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
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			CheckPrivateTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			PrivateServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PrivateTopicName);
			PublicServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
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
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			try
			{
				InstantiateReceiving(namespaceManager, PrivateServiceBusReceivers, PrivateTopicName, PrivateTopicSubscriptionName);
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the private Service Bus receivers may be invalid.", exception);
			}
			try
			{
				InstantiateReceiving(namespaceManager, PublicServiceBusReceivers, PublicTopicName, PublicTopicSubscriptionName);
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

		/// <summary>
		/// Creates <see cref="AzureBus{TAuthenticationToken}.NumberOfReceiversCount"/> <see cref="SubscriptionClient"/>.
		/// If flushing is required, any flushed <see cref="SubscriptionClient"/> has <see cref="ClientEntity.Close()"/> called on it first.
		/// </summary>
		/// <param name="namespaceManager">The <see cref="NamespaceManager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="SubscriptionClient"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
		protected virtual void InstantiateReceiving(NamespaceManager namespaceManager, IDictionary<int, SubscriptionClient> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			for (int i = 0; i < NumberOfReceiversCount; i++)
			{
				SubscriptionClient serviceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicName, topicSubscriptionName);
				if (serviceBusReceivers.ContainsKey(i))
					serviceBusReceivers[i] = serviceBusReceiver;
				else
					serviceBusReceivers.Add(i, serviceBusReceiver);
			}
			// Remove any if the number has decreased
			for (int i = NumberOfReceiversCount; i < serviceBusReceivers.Count; i++)
			{
				SubscriptionClient serviceBusReceiver;
				if (serviceBusReceivers.TryGetValue(i, out serviceBusReceiver))
					serviceBusReceiver.Close();
				serviceBusReceivers.Remove(i);
			}
		}

		/// <summary>
		/// Checks if the private topic and subscription name exists as per <see cref="PrivateTopicName"/> and <see cref="PrivateTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="namespaceManager">The <see cref="NamespaceManager"/>.</param>
		protected virtual void CheckPrivateTopicExists(NamespaceManager namespaceManager)
		{
			CheckTopicExists(namespaceManager, PrivateTopicName = ConfigurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName, PrivateTopicSubscriptionName = ConfigurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName);
		}

		/// <summary>
		/// Checks if the public topic and subscription name exists as per <see cref="PublicTopicName"/> and <see cref="PublicTopicSubscriptionName"/>.
		/// </summary>
		/// <param name="namespaceManager">The <see cref="NamespaceManager"/>.</param>
		protected virtual void CheckPublicTopicExists(NamespaceManager namespaceManager)
		{
			CheckTopicExists(namespaceManager, PublicTopicName = ConfigurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName, PublicTopicSubscriptionName = ConfigurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName);
		}

		/// <summary>
		/// Checks if a topic by the provided <paramref name="topicName"/> exists and
		/// Checks if a subscription name by the provided <paramref name="subscriptionName"/> exists.
		/// </summary>
		protected virtual void CheckTopicExists(NamespaceManager namespaceManager, string topicName, string subscriptionName)
		{
			// Configure Queue Settings
			var eventTopicDescription = new TopicDescription(topicName)
			{
				MaxSizeInMegabytes = 5120,
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true
			};
			// Create the topic if it does not exist already
			if (!namespaceManager.TopicExists(eventTopicDescription.Path))
				namespaceManager.CreateTopic(eventTopicDescription);

			if (!namespaceManager.SubscriptionExists(eventTopicDescription.Path, subscriptionName))
				namespaceManager.CreateSubscription
				(
					new SubscriptionDescription(eventTopicDescription.Path, subscriptionName)
					{
						DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
						EnableBatchedOperations = true,
						EnableDeadLetteringOnFilterEvaluationExceptions = true
					}
				);
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
		protected virtual void TriggerSettingsChecking(TopicClient serviceBusPublisher, IDictionary<int, SubscriptionClient> serviceBusReceivers)
		{
			// Let's wrap up using this message bus and start the switch
			if (serviceBusPublisher != null)
			{
				serviceBusPublisher.Close();
				Logger.LogDebug("Publishing service bus closed.");
			}
			foreach (SubscriptionClient serviceBusReceiver in serviceBusReceivers.Values)
			{
				// Let's wrap up using this message bus and start the switch
				if (serviceBusReceiver != null)
				{
					serviceBusReceiver.Close();
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
		protected virtual void RegisterReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
		{
			StoreReceiverMessageHandler(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();
		}

		/// <summary>
		/// Stores the provided <paramref name="receiverMessageHandler"/> and <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
		protected virtual void StoreReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
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
			foreach (SubscriptionClient serviceBusReceiver in PrivateServiceBusReceivers.Values)
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
			foreach (SubscriptionClient serviceBusReceiver in PublicServiceBusReceivers.Values)
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

			Action<BrokeredMessage, IMessage> leaveDeadlLetterInQueue = (deadLetterBrokeredMessage, deadLetterMessage) =>
			{
				// Remove message from queue
				try
				{
					deadLetterBrokeredMessage.Abandon();
					lockIssues = 0;
				}
				catch (MessageLockLostException)
				{
					lockIssues++;
					Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", deadLetterBrokeredMessage.MessageId));
				}
				Logger.LogDebug(string.Format("A dead-letter message of type {0} arrived with the id '{1}' but left in the queue due to settings.", deadLetterMessage.GetType().FullName, deadLetterBrokeredMessage.MessageId));
			};
			Action<BrokeredMessage> removeDeadlLetterFromQueue = deadLetterBrokeredMessage =>
			{
				// Remove message from queue
				try
				{
					deadLetterBrokeredMessage.Complete();
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
					MessagingFactory factory = MessagingFactory.CreateFromConnectionString(ConnectionString);
					string deadLetterPath = SubscriptionClient.FormatDeadLetterPath(topicName, topicSubscriptionName);
					MessageReceiver client = factory.CreateMessageReceiver(deadLetterPath, ReceiveMode.PeekLock);

					IEnumerable<BrokeredMessage> brokeredMessages = client.ReceiveBatch(1000);

					foreach (BrokeredMessage brokeredMessage in brokeredMessages)
					{
						if (lockIssues > 10)
							break;
						try
						{
							Logger.LogDebug(string.Format("A dead-letter message arrived with the id '{0}'.", brokeredMessage.MessageId));
							string messageBody = brokeredMessage.GetBody<string>();

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
											removeDeadlLetterFromQueue(message);
										else
											leaveDeadlLetterInQueue(message, @event);
										return true;
									},
									string.Format("id '{0}'", brokeredMessage.MessageId),
									() =>
									{
										removeDeadlLetterFromQueue(message);
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
											removeDeadlLetterFromQueue(message);
										else
											leaveDeadlLetterInQueue(message, command);
										return true;
									},
									string.Format("id '{0}'", brokeredMessage.MessageId),
									() =>
									{
										removeDeadlLetterFromQueue(message);
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
								brokeredMessage.Abandon();
							}
							catch (MessageLockLostException)
							{
								lockIssues++;
								Logger.LogWarning(string.Format("The lock supplied for abandon for the skipped dead-letter message '{0}' is invalid.", brokeredMessage.MessageId));
							}
						}
					}

					client.Close();

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
	}
}