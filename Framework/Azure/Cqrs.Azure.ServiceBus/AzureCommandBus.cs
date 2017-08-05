#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Bus;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureCommandBus<TAuthenticationToken> : AzureServiceBus<TAuthenticationToken>
	{
		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override string MessageBusConnectionStringConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.ConnectionString"; }
		}

		protected override string PrivateTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PrivateEvent.TopicName"; }
		}

		protected override string PublicTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PublicEvent.TopicName"; }
		}

		protected override string PrivateTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PrivateEvent.TopicName.SubscriptionName"; }
		}

		protected override string PublicTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PublicEvent.TopicName.SubscriptionName"; }
		}

		protected override string DefaultPrivateTopicName
		{
			get { return "Cqrs.CommandBus.Private"; }
		}

		protected override string DefaultPublicTopicName
		{
			get { return "Cqrs.CommandBus"; }
		}

		protected override string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete"; }
		}

		#endregion

		protected AzureCommandBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, isAPublisher)
		{
		}
	}
}
