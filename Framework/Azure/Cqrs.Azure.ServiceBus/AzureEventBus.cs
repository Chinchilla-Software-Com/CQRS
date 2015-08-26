using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureEventBus<TAuthenticationToken> : AzureBus<TAuthenticationToken>
	{
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

		protected AzureEventBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper CorrelationIdHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, CorrelationIdHelper, false, true)
		{
		}
	}
}
