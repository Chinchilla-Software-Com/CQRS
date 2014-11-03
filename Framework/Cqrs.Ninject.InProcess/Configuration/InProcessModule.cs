using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.InProcess.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class InProcessModule<TAuthenticationToken> : NinjectModule
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
			var inProcessBus = new InProcessBus<TAuthenticationToken>(Kernel.Get<IAuthenticationTokenHelper<TAuthenticationToken>>());

			Bind<ICommandSender<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			Bind<IEventPublisher<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			Bind<IHandlerRegistrar>()
				.ToConstant(inProcessBus)
				.InSingletonScope();
		}
	}
}
