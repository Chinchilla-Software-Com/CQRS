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
	/// A command bus based on <see cref="AzureServiceBus{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class AzureCommandBus<TAuthenticationToken>
		: AzureServiceBus<TAuthenticationToken>
	{
		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// The configuration key for the message bus connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string MessageBusConnectionStringConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.ConnectionString"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection endpoint as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string MessageBusConnectionEndpointConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.Connection.Endpoint"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection Application Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string MessageBusConnectionApplicationIdConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.Connection.ApplicationId"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection Client Key/Secret as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string MessageBusConnectionClientKeyConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.Connection.ClientKey"; }
		}

		/// <summary>
		/// The configuration key for the message bus connection Tenant Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected override string MessageBusConnectionTenantIdConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.Connection.TenantId"; }
		}

		/// <summary>
		/// The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string SigningTokenConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.SigningToken"; }
		}

		/// <summary>
		/// The configuration key for the name of the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PrivateCommand.Topic.Name"; }
		}

		/// <summary>
		/// The configuration key for the name of the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PublicCommand.Topic.Name"; }
		}

		/// <summary>
		/// The configuration key for the name of the subscription in the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PrivateCommand.Topic.Subscription.Name"; }
		}

		/// <summary>
		/// The configuration key for the name of the subscription in the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PublicCommand.Topic.Subscription.Name"; }
		}

		/// <summary>
		/// The default name of the private topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPrivateTopicName
		{
			get { return "Cqrs.CommandBus.Private"; }
		}

		/// <summary>
		/// The default name of the public topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPublicTopicName
		{
			get { return "Cqrs.CommandBus"; }
		}

		/// <summary>
		/// The configuration key that
		/// specifies if an <see cref="Exception"/> is thrown if the network lock is lost
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete"; }
		}

		#endregion

		/// <summary>
		/// Instantiate a new instance of <see cref="AzureCommandBus{TAuthenticationToken}"/>.
		/// </summary>
		protected AzureCommandBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, isAPublisher)
		{
		}
	}
}