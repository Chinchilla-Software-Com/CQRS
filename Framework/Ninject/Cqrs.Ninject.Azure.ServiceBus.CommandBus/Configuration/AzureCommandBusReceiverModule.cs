using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class AzureCommandBusReceiverModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterCommandHandlerRegistrar();
		}

		#endregion

		/// <summary>
		/// Register the Cqrs command handler registrar
		/// </summary>
		public virtual void RegisterCommandHandlerRegistrar()
		{
			Bind<ICommandHandlerRegistrar>()
				.To<AzureCommandBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}
