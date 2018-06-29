#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Bus;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A event bus based on <see cref="AzureEventHub{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class AzureEventHubBus<TAuthenticationToken>
		: AzureEventHub<TAuthenticationToken>
	{
		/// <summary>
		/// The configuration key for the event hub connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string EventHubConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.ConnectionStringName"; }
		}

		/// <summary>
		/// The configuration key for the event hub storage connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string EventHubStorageConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.StorageConnectionStringName"; }
		}

		/// <summary>
		/// The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string SigningTokenConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.SigningToken"; }
		}

		/// <summary>
		/// The configuration key for the name of the private event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PrivateEvent.EventHubName"; }
		}

		/// <summary>
		/// The configuration key for the name of the public event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PublicEvent.EventHubName"; }
		}

		/// <summary>
		/// The configuration key for the name of the consumer group name of the private event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PrivateEvent.EventHubName.ConsumerGroupName"; }
		}

		/// <summary>
		/// The configuration key for the name of the consumer group name of the public event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PublicEvent.EventHubName.ConsumerGroupName"; }
		}

		/// <summary>
		/// The default name of the private event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPrivateEventHubName
		{
			get { return "Cqrs.EventHub.EventBus.Private"; }
		}

		/// <summary>
		/// The default name of the public event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPublicEventHubName
		{
			get { return "Cqrs.EventHub.EventBus"; }
		}

		/// <summary>
		/// Gets the <see cref="IAzureBusHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="AzureEventHubBus{TAuthenticationToken}"/>.
		/// </summary>
		protected AzureEventHubBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IHashAlgorithmFactory hashAlgorithmFactory, IAzureBusHelper<TAuthenticationToken> azureBusHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, hashAlgorithmFactory, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
		}
	}
}