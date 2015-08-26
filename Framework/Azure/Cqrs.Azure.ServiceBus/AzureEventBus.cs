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

		protected override string PrivateQueueNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PrivateEvent.QueueName"; }
		}

		protected override string PublicQueueNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventBus.PublicEvent.QueueName"; }
		}

		protected override string DefaultPrivateQueueName
		{
			get { return "Cqrs.EventBus.Private"; }
		}

		protected override string DefaultPublicQueueName
		{
			get { return "Cqrs.EventBus"; }
		}

		protected AzureEventBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper CorrelationIdHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, CorrelationIdHelper)
		{
		}
	}
}
