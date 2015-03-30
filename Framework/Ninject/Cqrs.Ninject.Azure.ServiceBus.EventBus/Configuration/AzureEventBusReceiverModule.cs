using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class AzureEventBusReceiverModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterEventHandlerRegistrar();
		}

		#endregion

		/// <summary>
		/// Register the Cqrs event handler registrar
		/// </summary>
		public virtual void RegisterEventHandlerRegistrar()
		{
			Bind<IEventHandlerRegistrar>()
				.To<AzureEventBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}
