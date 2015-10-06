using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureCommandBus<TAuthenticationToken> : AzureBus<TAuthenticationToken>
	{
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

		protected AzureCommandBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
		}
	}
}
