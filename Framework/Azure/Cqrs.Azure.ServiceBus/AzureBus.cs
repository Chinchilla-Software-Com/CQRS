using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Bus;
using Cqrs.Events;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using RetryPolicy = Microsoft.Practices.TransientFaultHandling.RetryPolicy;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureBus<TAuthenticationToken>
	{
		protected string ConnectionString { get; private set; }

		protected string PrivateTopicName { get; private set; }

		protected string PublicTopicName { get; private set; }

		protected string PrivateTopicSubscriptionName { get; private set; }

		protected string PublicTopicSubscriptionName { get; private set; }

		protected IMessageSerialiser<TAuthenticationToken> MessageSerialiser { get; private set; }

		protected TopicClient ServiceBusPublisher { get; private set; }

		protected SubscriptionClient ServiceBusReceiver { get; private set; }

		protected abstract string MessageBusConnectionStringConfigurationKey { get; }

		protected abstract string PrivateTopicNameConfigurationKey { get; }

		protected abstract string PublicTopicNameConfigurationKey { get; }

		protected abstract string DefaultPrivateTopicName { get; }

		protected abstract string DefaultPublicTopicName { get; }

		protected abstract string PrivateTopicSubscriptionNameConfigurationKey { get; }

		protected abstract string PublicTopicSubscriptionNameConfigurationKey { get; }

		protected const string DefaultPrivateTopicSubscriptionName= "Root";

		protected const string DefaultPublicTopicSubscriptionName = "Root";

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected IBusHelper BusHelper { get; private set; }

		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> EventWaits { get; private set; }

		protected Action<BrokeredMessage> ReceiverMessageHandler { get; private set; }

		protected OnMessageOptions ReceiverMessageHandlerOptions { get; private set; }

		protected AzureBus(IConfigurationManager configurationManager, IBusHelper busHelper, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher)
		{
			EventWaits = new ConcurrentDictionary<Guid, IList<IEvent<TAuthenticationToken>>>();

			MessageSerialiser = messageSerialiser;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			BusHelper = busHelper;
			ConfigurationManager = configurationManager;
			ConnectionString = ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);

			if (isAPublisher)
				InstantiatePublishing();
		}

		protected void InstantiatePublishing()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			ServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
			TriggerConnectionSettingsChecking();
		}

		protected void InstantiateReceiving()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateEventTopicExists(namespaceManager);
			CheckPublicTopicExists(namespaceManager);

			ServiceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, PublicTopicName, PublicTopicSubscriptionName);

			// If this is also a publisher, then it will the check there will handle this
			if (ServiceBusPublisher != null)
				return;

			TriggerConnectionSettingsChecking();
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

		/// <summary>
		/// Gets the default retry policy dedicated to handling transient conditions with Windows Azure Service Bus.
		/// </summary>
		protected virtual RetryPolicy AzureServiceBusRetryPolicy
		{
			get
			{
				RetryManager retryManager = EnterpriseLibraryContainer.Current.GetInstance<RetryManager>();
				RetryPolicy retryPolicy = retryManager.GetDefaultAzureServiceBusRetryPolicy();
				retryPolicy.Retrying += (sender, args) =>
				{
					var message = string.Format("Retrying action - Count:{0}, Delay:{1}", args.CurrentRetryCount, args.Delay);
					Logger.LogWarning(message, "AzureServiceBusRetryPolicy", args.LastException);
				};
				return retryPolicy;
			}
		}

		protected virtual void TriggerConnectionSettingsChecking()
		{
			Task.Factory.StartNew(() =>
			{
				SpinWait.SpinUntil(() => ConnectionString != ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey));

				Logger.LogInfo("Connecting string settings for the Azure Service Bus changed and will now refresh.");

				// Update the connection string and trigger a restart;
				ConnectionString = ConfigurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);
				Logger.LogDebug(string.Format("Updated connecting string settings set to {0}.", ConnectionString));

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
						ServiceBusReceiver.OnMessage(ReceiverMessageHandler, ReceiverMessageHandlerOptions);
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
			});
		}

		protected virtual void RegisterReceiverMessageHandler(Action<BrokeredMessage> receiverMessageHandler, OnMessageOptions receiverMessageHandlerOptions)
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;

			ServiceBusReceiver.OnMessage(ReceiverMessageHandler, ReceiverMessageHandlerOptions);
		}
	}
}