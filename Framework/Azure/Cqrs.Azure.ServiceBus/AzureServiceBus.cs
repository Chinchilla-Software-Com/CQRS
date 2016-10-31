#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
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

		protected SubscriptionClient ServiceBusReceiver { get; private set; }

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
		}

		#region Overrides of AzureBus<TAuthenticationToken>

		protected override void SetConnectionStrings()
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor 
			ConnectionString = ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		#endregion

		protected override void InstantiatePublishing()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			ServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
			StartConnectionSettingsChecking();
		}

		protected override void InstantiateReceiving()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			ServiceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, PublicTopicName, PublicTopicSubscriptionName);

			// If this is also a publisher, then it will the check over there and that will handle this
			if (ServiceBusPublisher != null)
				return;

			StartConnectionSettingsChecking();
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
				DefaultMessageTimeToLive = new TimeSpan(0, 1, 0)
			};
			// Create the topic if it does not exist already
			if (!namespaceManager.TopicExists(eventTopicDescription.Path))
				namespaceManager.CreateTopic(eventTopicDescription.Path);

			if (!namespaceManager.SubscriptionExists(eventTopicDescription.Path, eventSubscriptionNames))
				namespaceManager.CreateSubscription(eventTopicDescription.Path, eventSubscriptionNames);
		}

		protected override bool ValidateConnectionSettingHasChanged()
		{
			return ConnectionString != ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);
		}

		protected override void UpdateConnectionSettings()
		{
			ConnectionString = ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);
			Logger.LogDebug(string.Format("Updated connection string settings set to {0}.", ConnectionString));
		}

		protected override void TriggerConnectionSettingsChecking()
		{
			// Let's wrap up using this message bus and start the switch
			if (ServiceBusPublisher != null)
			{
				ServiceBusPublisher.Close();
				Logger.LogDebug("Publishing service bus closed.");
			}
			// Let's wrap up using this message bus and start the switch
			if (ServiceBusReceiver != null)
			{
				ServiceBusReceiver.Close();
				Logger.LogDebug("Receiving service bus closed.");
			}
			// Restart configuration, we order this intentionally with the receiver first as if this triggers the cancellation we know this isn't a publisher as well
			if (ServiceBusReceiver != null)
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
			ServiceBusReceiver.OnMessage(ReceiverMessageHandler, ReceiverMessageHandlerOptions);
		}
	}
}