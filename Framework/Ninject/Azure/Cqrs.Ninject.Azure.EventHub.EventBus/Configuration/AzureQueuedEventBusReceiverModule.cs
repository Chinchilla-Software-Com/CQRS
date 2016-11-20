using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;

namespace Cqrs.Azure.EventHub.EventBus.Configuration
{
	public class AzureQueuedEventBusReceiverModule<TAuthenticationToken> : AzureEventBusReceiverModule<TAuthenticationToken>
	{
		public override void RegisterEventHandlerRegistrar()
		{
			Bind<IEventHandlerRegistrar>()
				.To<AzureQueuedEventBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}