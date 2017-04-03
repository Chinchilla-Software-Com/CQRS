#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Bus;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureEventBus<TAuthenticationToken> : AzureServiceBus<TAuthenticationToken>
	{
		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override string MessageBusConnectionStringConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.ConnectionString"; }
		}

		protected override string PrivateTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PrivateEvent.TopicName"; }
		}

		protected override string PublicTopicNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PublicEvent.TopicName"; }
		}

		protected override string PrivateTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PrivateEvent.TopicName.SubscriptionName"; }
		}

		protected override string PublicTopicSubscriptionNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PublicEvent.TopicName.SubscriptionName"; }
		}

		protected override string DefaultPrivateTopicName
		{
			get { return "Cqrs.EventBus.Private"; }
		}

		protected override string DefaultPublicTopicName
		{
			get { return "Cqrs.EventBus"; }
		}

		protected override string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete"; }
		}

		#endregion

		protected AzureEventBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, isAPublisher)
		{
		}
	}
}
