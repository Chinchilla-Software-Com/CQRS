using System.Linq;
using Cqrs.Azure.ServiceBus;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	public class AzureQueuedCommandBusReceiverModule<TAuthenticationToken> : AzureCommandBusReceiverModule<TAuthenticationToken>
	{
		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			bool isMessageSerialiserBound = Kernel.GetBindings(typeof(IAzureBusHelper<TAuthenticationToken>)).Any();
			if (!isMessageSerialiserBound)
			{
				Bind<IAzureBusHelper<TAuthenticationToken>>()
					.To<AzureBusHelper<TAuthenticationToken>>()
					.InSingletonScope();
			}

			var bus = GetOrCreateBus<AzureQueuedCommandBusReceiver<TAuthenticationToken>>();

			RegisterCommandReceiver(bus);
			RegisterCommandHandlerRegistrar(bus);
			RegisterCommandMessageSerialiser();
		}
	}
}