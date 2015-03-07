using System;
using Cqrs.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureMessageBus<TAuthenticationToken>
	{
		protected string ConnectionString { get; private set; }

		protected string PrivateEventQueueName { get; private set; }

		protected string PublicEventQueueName { get; private set; }

		protected IEventSerialiser<TAuthenticationToken> EventSerialiser { get; private set; }

		protected QueueClient ServiceBusClient { get; private set; }

		protected AzureMessageBus(IConfigurationManager configurationManager, IEventSerialiser<TAuthenticationToken> eventSerialiser)
		{
			EventSerialiser = eventSerialiser;
			// Create the queue if it does not exist already
			ConnectionString = configurationManager.GetSetting("Cqrs.Azure.MessageBus.ConnectionString");

			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

			CheckPrivateEventQueue(configurationManager, namespaceManager);
			CheckPublicEventQueue(configurationManager, namespaceManager);

			ServiceBusClient = QueueClient.CreateFromConnectionString(ConnectionString, PublicEventQueueName);
		}

		protected virtual void CheckPrivateEventQueue(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckEventQueueExists(namespaceManager, PrivateEventQueueName = configurationManager.GetSetting("Cqrs.Azure.MessageBus.PrivateEvent.QueueName") ?? "Cqrs.MessageBus.Private");
		}

		protected virtual void CheckPublicEventQueue(IConfigurationManager configurationManager, NamespaceManager namespaceManager)
		{
			CheckEventQueueExists(namespaceManager, PublicEventQueueName = configurationManager.GetSetting("Cqrs.Azure.MessageBus.PublicEvent.QueueName") ?? "Cqrs.MessageBus");
		}

		protected virtual void CheckEventQueueExists(NamespaceManager namespaceManager, string eventQueueName)
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
