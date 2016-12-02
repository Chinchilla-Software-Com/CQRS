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
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
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

		protected const string DefaultPrivateTopicSubscriptionName = "Root";

		protected const string DefaultPublicTopicSubscriptionName = "Root";

		protected Action<BrokeredMessage> ReceiverMessageHandler { get; set; }

		protected OnMessageOptions ReceiverMessageHandlerOptions { get; set; }

		protected AzureServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher)
			: base (configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
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
				EnablePartitioning = true
			};
			// Create the topic if it does not exist already
			if (!namespaceManager.TopicExists(eventTopicDescription.Path))
				namespaceManager.CreateTopic(eventTopicDescription);

			if (!namespaceManager.SubscriptionExists(eventTopicDescription.Path, eventSubscriptionNames))
				namespaceManager.CreateSubscription(eventTopicDescription.Path, eventSubscriptionNames);
		}

		protected override void TriggerSettingsChecking()
		{
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
	}
}