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
		protected TopicClient ServiceBusPublisher { get; private set; }

		protected IDictionary<int, SubscriptionClient> ServiceBusReceivers { get; private set; }

		protected string PrivateTopicName { get; set; }

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
			ServiceBusReceivers = new Dictionary<int, SubscriptionClient>();
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

			ServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
			StartSettingsChecking();
		}

		protected override void InstantiateReceiving()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			for (int i = 0; i < NumberOfReceiversCount; i++)
			{
				SubscriptionClient serviceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, PublicTopicName, PublicTopicSubscriptionName);
				if (ServiceBusReceivers.ContainsKey(i))
					ServiceBusReceivers[i] = serviceBusReceiver;
				else
					ServiceBusReceivers.Add(i, serviceBusReceiver);
			}
			// Remove any if the number has decreased
			for (int i = NumberOfReceiversCount; i < ServiceBusReceivers.Count; i++)
				ServiceBusReceivers.Remove(i + 1);

			// If this is also a publisher, then it will the check over there and that will handle this
			if (ServiceBusPublisher != null)
				return;

			StartSettingsChecking();
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
			// Let's wrap up using this message bus and start the switch
			if (ServiceBusPublisher != null)
			{
				ServiceBusPublisher.Close();
				Logger.LogDebug("Publishing service bus closed.");
			}
			foreach (SubscriptionClient serviceBusReceiver in ServiceBusReceivers.Values)
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
			// Restart configuration, we order this intentionally with the publisher second as if this triggers the cancellation there's nothing else to process here
			if (ServiceBusPublisher != null)
			{
				Logger.LogDebug("Recursively calling into InstantiatePublishing.");
				InstantiatePublishing();
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
			foreach (SubscriptionClient serviceBusReceiver in ServiceBusReceivers.Values)
				serviceBusReceiver.OnMessage(ReceiverMessageHandler, ReceiverMessageHandlerOptions);
		}
	}
}