using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	public class AzureQueuedCommandBusReceiverModule<TAuthenticationToken> : AzureCommandBusReceiverModule<TAuthenticationToken>
	{
		public override void RegisterCommandHandlerRegistrar()
		{
			Bind<ICommandHandlerRegistrar>()
				.To<AzureQueuedCommandBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}