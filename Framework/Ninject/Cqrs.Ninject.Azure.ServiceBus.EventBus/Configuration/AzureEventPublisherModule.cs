using Cqrs.Azure.ServiceBus;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class AzureEventPublisherModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterEventPublisher();
		}

		#endregion

		/// <summary>
		/// Register the Cqrs event publisher
		/// </summary>
		public virtual void RegisterEventPublisher()
		{
			Bind<IEventPublisher<TAuthenticationToken>>()
				.To<AzureEventBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}
