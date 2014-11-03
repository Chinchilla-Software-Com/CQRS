using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.InProcess.EventBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class InProcessEventBusModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
			RegisterServices();
			RegisterCqrsRequirements();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register the all Cqrs command handlers
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			var inProcessBus = Kernel.TryGet<InProcessBus<TAuthenticationToken>>();
			if (inProcessBus == null)
			{
				inProcessBus = new InProcessBus<TAuthenticationToken>(Kernel.Get<IAuthenticationTokenHelper<TAuthenticationToken>>());
				Bind<InProcessBus<TAuthenticationToken>>()
					.ToConstant(inProcessBus)
					.InSingletonScope();
			}

			Bind<IEventPublisher<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			var handlerRegistration = Kernel.TryGet<IHandlerRegistrar>();
			if (handlerRegistration == null)
			{
				Bind<IHandlerRegistrar>()
					.ToConstant(inProcessBus)
					.InSingletonScope();
			}
		}
	}
}
