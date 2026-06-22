#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;
using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using OldManager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using OldIMessageReceiver = Microsoft.Azure.ServiceBus.Core.IMessageReceiver;
using OldTopicClient = Microsoft.Azure.ServiceBus.TopicClient;
#else
using OldManager = Microsoft.ServiceBus.NamespaceManager;
using OldIMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
using OldTopicClient = Microsoft.ServiceBus.Messaging.TopicClient;
#endif

namespace Cqrs.Azure.Functions.ServiceBus
{
	/// <summary>
	/// An <see cref="AzureBus{TAuthenticationToken}"/> that uses Azure Service Bus hosted in Azure Functions.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <remarks>
	/// https://markheath.net/post/migrating-to-new-servicebus-sdk
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#receive-messages-from-the-subscription
	/// https://stackoverflow.com/questions/47427361/azure-service-bus-read-messages-sent-by-net-core-2-with-brokeredmessage-getbo
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
	/// </remarks>
	public abstract class AzureFunctionServiceBus<TAuthenticationToken>
		: AzureServiceBus<TAuthenticationToken>
	{
		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureServiceBus{TAuthenticationToken}"/>
		/// </summary>
		protected AzureFunctionServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, isAPublisher)
		{
		}

		#endregion
	}
}