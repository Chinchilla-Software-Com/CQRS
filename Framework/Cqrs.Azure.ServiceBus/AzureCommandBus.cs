using Cqrs.Configuration;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureCommandBus<TAuthenticationToken> : AzureBus<TAuthenticationToken>
	{
		protected override string MessageBusConnectionStringConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.ConnectionString"; }
		}

		protected override string PrivateQueueNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PrivateEvent.QueueName"; }
		}

		protected override string PublicQueueNameConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.PublicEvent.QueueName"; }
		}

		protected override string DefaultPrivateQueueName
		{
			get { return "Cqrs.CommandBus.Private"; }
		}

		protected override string DefaultPublicQueueName
		{
			get { return "Cqrs.CommandBus"; }
		}

		protected AzureCommandBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser)
			: base(configurationManager, messageSerialiser)
		{
		}
	}
}
