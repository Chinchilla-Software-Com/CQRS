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
using Chinchilla.Logging;
using Cqrs.Bus;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A command bus based on <see cref="AzureEventHub{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class AzureCommandBus<TAuthenticationToken>
		: AzureEventHub<TAuthenticationToken>
	{
		/// <summary>
		/// The configuration key for the event hub connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string EventHubConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.ConnectionStringName"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection endpoint as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string EventHubConnectionEndpointConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.Connection.Endpoint"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection Application Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string EventHubConnectionApplicationIdConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.Connection.ApplicationId"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection Client Key/Secret as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string EventHubConnectionClientKeyConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.Connection.ClientKey"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection Tenant Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string EventHubConnectionTenantIdConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.Connection.TenantId"; }
		}

		/// <summary>
		/// The configuration key for the event hub storage connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string EventHubStorageConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.StorageConnectionStringName"; }
		}

		/// <summary>
		/// The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string SigningTokenConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.SigningToken"; }
		}

		/// <summary>
		/// The configuration key for the name of the private event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PrivateEvent.EventHubName"; }
		}

		/// <summary>
		/// The configuration key for the name of the public event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PublicEvent.EventHubName"; }
		}

		/// <summary>
		/// The configuration key for the name of the consumer group name of the private event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PrivateEvent.EventHubName.ConsumerGroupName"; }
		}

		/// <summary>
		/// The configuration key for the name of the consumer group name of the public event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PublicEvent.EventHubName.ConsumerGroupName"; }
		}

		/// <summary>
		/// The default name of the private event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPrivateEventHubName
		{
			get { return "Cqrs.EventHub.CommandBus.Private"; }
		}

		/// <summary>
		/// The default name of the public event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPublicEventHubName
		{
			get { return "Cqrs.EventHub.CommandBus"; }
		}

		/// <summary>
		/// Gets the <see cref="IAzureBusHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="AzureCommandBus{TAuthenticationToken}"/>.
		/// </summary>
		protected AzureCommandBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IHashAlgorithmFactory hashAlgorithmFactory, IAzureBusHelper<TAuthenticationToken> azureBusHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, hashAlgorithmFactory, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
		}
	}
}