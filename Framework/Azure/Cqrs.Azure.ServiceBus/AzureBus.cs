using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureBus<TAuthenticationToken>
	{
		protected string ConnectionString { get; private set; }

		protected string PrivateQueueName { get; private set; }

		protected string PublicQueueName { get; private set; }

		protected IMessageSerialiser<TAuthenticationToken> MessageSerialiser { get; private set; }

		protected QueueClient ServiceBusClient { get; private set; }

		protected abstract string MessageBusConnectionStringConfigurationKey { get; }

		protected abstract string PrivateQueueNameConfigurationKey { get; }

		protected abstract string PublicQueueNameConfigurationKey { get; }

		protected abstract string DefaultPrivateQueueName { get; }

		protected abstract string DefaultPublicQueueName { get; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected AzureBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper)
		{
			MessageSerialiser = messageSerialiser;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			// Create the queue if it does not exist already
			ConnectionString = configurationManager.GetSetting(MessageBusConnectionStringConfigurationKey);

			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateQueueExists(configurationManager, namespaceManager);
			CheckPublicQueueExists(configurationManager, namespaceManager);

			ServiceBusClient = QueueClient.CreateFromConnectionString(ConnectionString, PublicQueueName);
		}

		protected virtual void CheckPrivateQueueExists(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckQueueExists(namespaceManager, PrivateQueueName = configurationManager.GetSetting(PrivateQueueNameConfigurationKey) ?? DefaultPrivateQueueName);
		}

		protected virtual void CheckPublicQueueExists(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckQueueExists(namespaceManager, PublicQueueName = configurationManager.GetSetting(PublicQueueNameConfigurationKey) ?? DefaultPublicQueueName);
		}

		protected virtual void CheckQueueExists(NamespaceManager namespaceManager, string eventQueueName)
		{
			// Configure Queue Settings
			QueueDescription eventQueueDescription = new QueueDescription(eventQueueName)
			{
				MaxSizeInMegabytes = 5120,
				DefaultMessageTimeToLive = new TimeSpan(0, 1, 0)
			};
			if (!namespaceManager.QueueExists(eventQueueDescription.Path))
				namespaceManager.CreateQueue(eventQueueDescription);
		}
	}
}
