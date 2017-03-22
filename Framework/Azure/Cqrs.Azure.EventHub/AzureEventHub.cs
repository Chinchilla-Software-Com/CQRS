#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureEventHub<TAuthenticationToken> : AzureBus<TAuthenticationToken>
	{
		protected EventHubClient EventHubPublisher { get; private set; }

		protected EventProcessorHost EventHubReceiver { get; private set; }

		protected string PrivateEventHubName { get; set; }

		protected string PublicEventHubName { get; private set; }

		protected string PrivateEventHubConsumerGroupName { get; private set; }

		protected string PublicEventHubConsumerGroupName { get; private set; }

		protected abstract string EventHubConnectionStringNameConfigurationKey { get; }

		protected abstract string EventHubStorageConnectionStringNameConfigurationKey { get; }

		protected abstract string PrivateEventHubNameConfigurationKey { get; }

		protected abstract string PublicEventHubNameConfigurationKey { get; }

		protected abstract string DefaultPrivateEventHubName { get; }

		protected abstract string DefaultPublicEventHubName { get; }

		protected abstract string PrivateEventHubConsumerGroupNameConfigurationKey { get; }

		protected abstract string PublicEventHubConsumerGroupNameConfigurationKey { get; }

		protected const string DefaultPrivateEventHubConsumerGroupName = "$Default";

		protected const string DefaultPublicEventHubConsumerGroupName = "$Default";

		protected string StorageConnectionString { get; private set; }

		protected Action<PartitionContext, EventData> ReceiverMessageHandler { get; private set; }
		
		protected EventProcessorOptions ReceiverMessageHandlerOptions { get; private set; }

		protected ITelemetryHelper TelemetryHelper { get; set; }

		protected AzureEventHub(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher)
			: base (configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			TelemetryHelper = new NullTelemetryHelper();
		}

		#region Overrides of AzureBus<TAuthenticationToken>

		protected override string GetConnectionString()
		{
			string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(EventHubConnectionStringNameConfigurationKey)].ConnectionString;
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ConfigurationErrorsException(string.Format("Configuration is missing required information. Make sure the appSetting '{0}' is defined and a matching connection string with the name that matches the value of the appSetting value '{0}'.", EventHubConnectionStringNameConfigurationKey));
			return connectionString;
		}

		protected override void SetConnectionStrings()
		{
			base.SetConnectionStrings();
			StorageConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(EventHubStorageConnectionStringNameConfigurationKey)].ConnectionString;
			if (string.IsNullOrWhiteSpace(StorageConnectionString))
				throw new ConfigurationErrorsException(string.Format("Configuration is missing required information. Make sure the appSetting '{0}' is defined and a matching connection string with the name that matches the value of the appSetting value '{0}'.", EventHubStorageConnectionStringNameConfigurationKey));
			Logger.LogDebug(string.Format("Storage connection string settings set to {0}.", StorageConnectionString));
		}

		#endregion

		protected override void InstantiatePublishing()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			CheckPrivateEventHubExists(namespaceManager);
			CheckPublicHubExists(namespaceManager);

			EventHubPublisher = EventHubClient.CreateFromConnectionString(ConnectionString, PublicEventHubName);
			StartSettingsChecking();
		}

		protected override void InstantiateReceiving()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateEventHubExists(namespaceManager);
			CheckPublicHubExists(namespaceManager);

			EventHubReceiver = new EventProcessorHost(PublicEventHubName, PublicEventHubConsumerGroupName, ConnectionString, StorageConnectionString);

			// If this is also a publisher, then it will the check over there and that will handle this
			if (EventHubPublisher != null)
				return;

			StartSettingsChecking();
		}

		protected virtual void CheckPrivateEventHubExists(NamespaceManager namespaceManager)
		{
			CheckHubExists(namespaceManager, PrivateEventHubName = ConfigurationManager.GetSetting(PrivateEventHubNameConfigurationKey) ?? DefaultPrivateEventHubName, PrivateEventHubConsumerGroupName = ConfigurationManager.GetSetting(PrivateEventHubConsumerGroupNameConfigurationKey) ?? DefaultPrivateEventHubConsumerGroupName);
		}

		protected virtual void CheckPublicHubExists(NamespaceManager namespaceManager)
		{
			CheckHubExists(namespaceManager, PublicEventHubName = ConfigurationManager.GetSetting(PublicEventHubNameConfigurationKey) ?? DefaultPublicEventHubName, PublicEventHubConsumerGroupName = ConfigurationManager.GetSetting(PublicEventHubConsumerGroupNameConfigurationKey) ?? DefaultPublicEventHubConsumerGroupName);
		}

		protected virtual void CheckHubExists(NamespaceManager namespaceManager, string eventHubName, string eventSubscriptionNames)
		{
			// Configure Queue Settings
			var eventHubDescription = new EventHubDescription(eventHubName)
			{
				MessageRetentionInDays = long.MaxValue,
				
			};

			// Create the topic if it does not exist already
			namespaceManager.CreateEventHubIfNotExists(eventHubDescription);

			var subscriptionDescription = new SubscriptionDescription(eventHubDescription.Path, eventSubscriptionNames);

			if (!namespaceManager.SubscriptionExists(eventHubDescription.Path, eventSubscriptionNames))
				namespaceManager.CreateSubscription(subscriptionDescription);
		}

		protected override bool ValidateSettingsHaveChanged()
		{
			return base.ValidateSettingsHaveChanged()
				||
			StorageConnectionString != ConfigurationManager.GetSetting(EventHubStorageConnectionStringNameConfigurationKey);
		}

		protected override void TriggerSettingsChecking()
		{
			// Let's wrap up using this event hub and start the switch
			if (EventHubPublisher != null)
			{
				EventHubPublisher.Close();
				Logger.LogDebug("Publishing event hub closed.");
			}
			// Let's wrap up using this event hub and start the switch
			if (EventHubReceiver != null)
			{
				Task work = EventHubReceiver.UnregisterEventProcessorAsync();
				work.ConfigureAwait(false);
				work.Wait();
				Logger.LogDebug("Receiving event hub closed.");
			}
			// Restart configuration, we order this intentionally with the receiver first as if this triggers the cancellation we know this isn't a publisher as well
			if (EventHubReceiver != null)
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
			if (EventHubPublisher != null)
			{
				Logger.LogDebug("Recursively calling into InstantiatePublishing.");
				InstantiatePublishing();
			}
		}

		protected virtual void RegisterReceiverMessageHandler(Action<PartitionContext, EventData> receiverMessageHandler, EventProcessorOptions receiverMessageHandlerOptions = null)
		{
			StoreReceiverMessageHandler(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();
		}

		protected virtual void StoreReceiverMessageHandler(Action<PartitionContext, EventData> receiverMessageHandler, EventProcessorOptions receiverMessageHandlerOptions = null)
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;
		}

		protected override void ApplyReceiverMessageHandler()
		{
			EventHubReceiver.RegisterEventProcessorFactoryAsync
			(
				new DefaultEventProcessorFactory<DefaultEventProcessor>
				(
					new DefaultEventProcessor(Logger, ReceiverMessageHandler)
				),
				ReceiverMessageHandlerOptions ?? EventProcessorOptions.DefaultOptions
			);
		}
	}
}