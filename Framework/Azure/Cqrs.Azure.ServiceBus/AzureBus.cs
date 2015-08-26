using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using Microsoft.Practices.TransientFaultHandling;
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

		protected AzureBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher, bool IsAReceiver)
		{
			MessageSerialiser = messageSerialiser;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			ConnectionString = configurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);

			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			if (isAPublisher)
				InstantiatePublishing(configurationManager, namespaceManager);

			if (IsAReceiver)
				InstantiateReceiving(configurationManager, namespaceManager);
		}

		protected void InstantiatePublishing(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckPrivateEventTopicExists(configurationManager, namespaceManager);
			CheckPublicTopicExists(configurationManager, namespaceManager);

			ServiceBusPublisher = TopicClient.CreateFromConnectionString(ConnectionString, PublicTopicName);
		}

		protected void InstantiateReceiving(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckPrivateEventTopicExists(configurationManager, namespaceManager);
			CheckPublicTopicExists(configurationManager, namespaceManager);

			ServiceBusReceiver = SubscriptionClient.CreateFromConnectionString(ConnectionString, PublicTopicName, PrivateTopicSubscriptionName);
		}

		protected virtual void CheckPrivateEventTopicExists(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckTopicExists(namespaceManager, PrivateTopicName = configurationManager.GetSetting(PrivateTopicNameConfigurationKey) ?? DefaultPrivateTopicName, PrivateTopicSubscriptionName = configurationManager.GetSetting(PrivateTopicSubscriptionNameConfigurationKey) ?? DefaultPrivateTopicSubscriptionName);
		}

		protected virtual void CheckPublicTopicExists(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckTopicExists(namespaceManager, PublicTopicName = configurationManager.GetSetting(PublicTopicNameConfigurationKey) ?? DefaultPublicTopicName, PublicTopicSubscriptionName = configurationManager.GetSetting(PublicTopicSubscriptionNameConfigurationKey) ?? DefaultPublicTopicSubscriptionName);
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
	}
}