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
	/// A event bus based on <see cref="AzureServiceBus{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class AzureEventBus<TAuthenticationToken> : AzureServiceBus<TAuthenticationToken>
	{
		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// The configuration key for the message bus connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string MessageBusConnectionStringConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.ConnectionString"; }
		}

		/// <summary>
		/// The configuration key for the name of the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PrivateEvent.TopicName"; }
		}

		/// <summary>
		/// The configuration key for the name of the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PublicEvent.TopicName"; }
		}

		/// <summary>
		/// The configuration key for the name of the subscription in the private topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PrivateTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PrivateEvent.TopicName.SubscriptionName"; }
		}

		/// <summary>
		/// The configuration key for the name of the subscription in the public topic as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string PublicTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PublicEvent.TopicName.SubscriptionName"; }
		}

		/// <summary>
		/// The default name of the private topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPrivateTopicName
		{
			get { return "Cqrs.EventBus.Private"; }
		}

		/// <summary>
		/// The default name of the public topic if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected override string DefaultPublicTopicName
		{
			get { return "Cqrs.EventBus"; }
		}

		/// <summary>
		/// The configuration key that
		/// specifies if an <see cref="Exception"/> is thrown if the network lock is lost
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected override string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete"; }
		}

		#endregion

		/// <summary>
		/// Instantiate a new instance of <see cref="AzureEventBus{TAuthenticationToken}"/>.
		/// </summary>
		protected AzureEventBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, isAPublisher)
		{
		}
	}
}