#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
using Cqrs.Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureServiceBus<TAuthenticationToken> : AzureBus<TAuthenticationToken>
	{
		protected TopicClient PrivateServiceBusPublisher { get; private set; }

		protected TopicClient PublicServiceBusPublisher { get; private set; }

		protected IDictionary<int, SubscriptionClient> PrivateServiceBusReceivers { get; private set; }

		protected IDictionary<int, SubscriptionClient> PublicServiceBusReceivers { get; private set; }

		protected string PrivateTopicName { get; private set; }

		protected string PublicTopicName { get; private set; }

		protected string PrivateTopicSubscriptionName { get; private set; }

		protected string PublicTopicSubscriptionName { get; private set; }

		protected abstract string MessageBusConnectionStringConfigurationKey { get; }

		protected abstract string PrivateTopicNameConfigurationKey { get; }

		protected abstract string PublicTopicNameConfigurationKey { get; }

		protected abstract string DefaultPrivateTopicName { get; }

		protected abstract string DefaultPublicTopicName { get; }

		protected abstract string PrivateTopicSubscriptionNameConfigurationKey { get; }

		protected abstract string PublicTopicSubscriptionNameConfigurationKey { get; }

		protected abstract string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey { get; }

		protected bool ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete { get; private set; }

		protected const string DefaultPrivateTopicSubscriptionName = "Root";

		protected const string DefaultPublicTopicSubscriptionName = "Root";

		protected Action<BrokeredMessage> ReceiverMessageHandler { get; set; }

		protected OnMessageOptions ReceiverMessageHandlerOptions { get; set; }

		protected IBusHelper BusHelper { get; private set; }

		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }

		protected ITelemetryHelper TelemetryHelper { get; set; }

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

		protected override string GetConnectionString()
		{
			string connectionString = ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ConfigurationErrorsException(string.Format("Configuration is missing required information. Make sure the appSetting '{0}' is defined and has a valid connection string value.", MessageBusConnectionStringConfigurationKey));
			return connectionString;
		}

		#endregion

		protected override void InstantiatePublishing()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			PrivateServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PrivateTopicName);
			PublicServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
			StartSettingsChecking();
		}

		protected override void InstantiateReceiving()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			InstantiateReceiving(PrivateServiceBusReceivers, PrivateTopicName, PrivateTopicSubscriptionName);
			InstantiateReceiving(PublicServiceBusReceivers, PublicTopicName, PublicTopicSubscriptionName);

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

		protected virtual void InstantiateReceiving(IDictionary<int, SubscriptionClient> serviceBusReceivers, string topicName, string topicSubscriptionName)
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
				serviceBusReceivers.Remove(i + 1);
		}

		protected virtual void CheckPrivateEventTopicExists(NamespaceManager namespaceManager)
		{
			CheckTopicExists(namespaceManager, PrivateTopicName = ConfigurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName, PrivateTopicSubscriptionName = ConfigurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName);
		}

		protected virtual void CheckPublicTopicExists(NamespaceManager namespaceManager)
		{
			CheckTopicExists(namespaceManager, PublicTopicName = ConfigurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName, PublicTopicSubscriptionName = ConfigurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName);
		}

		protected virtual void CheckTopicExists(NamespaceManager namespaceManager, string eventTopicName, string eventSubscriptionNames)
		{
			// Configure Queue Settings
			var eventTopicDescription = new TopicDescription(eventTopicName)
			{
				MaxSizeInMegabytes = 5120,
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true
			};
			// Create the topic if it does not exist already
			if (!namespaceManager.TopicExists(eventTopicDescription.Path))
				namespaceManager.CreateTopic(eventTopicDescription);

			if (!namespaceManager.SubscriptionExists(eventTopicDescription.Path, eventSubscriptionNames))
				namespaceManager.CreateSubscription
				(
					new SubscriptionDescription(eventTopicDescription.Path, eventSubscriptionNames)
					{
						DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
						EnableBatchedOperations = true,
						EnableDeadLetteringOnFilterEvaluationExceptions = true
					}
				);
		}

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

		protected virtual void RegisterReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
		{
			StoreReceiverMessageHandler(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();
		}

		protected virtual void StoreReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;
		}

		protected override void ApplyReceiverMessageHandler()
		{
			foreach (SubscriptionClient serviceBusReceiver in PrivateServiceBusReceivers.Values)
				serviceBusReceiver.OnMessage(ReceiverMessageHandler, ReceiverMessageHandlerOptions);
			foreach (SubscriptionClient serviceBusReceiver in PublicServiceBusReceivers.Values)
				serviceBusReceiver.OnMessage(ReceiverMessageHandler, ReceiverMessageHandlerOptions);
		}

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