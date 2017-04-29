using System.Linq;
using Cqrs.Bus;
using Cqrs.Commands;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.InProcess.CommandBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class InProcessCommandBusModule<TAuthenticationToken> : NinjectModule
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
			bool isInProcessBusBound = Kernel.GetBindings(typeof(InProcessBus<TAuthenticationToken>)).Any();
			InProcessBus<TAuthenticationToken> inProcessBus;
			if (!isInProcessBusBound)
			{
				inProcessBus = Kernel.Get<InProcessBus<TAuthenticationToken>>();
				Bind<InProcessBus<TAuthenticationToken>>()
					.ToConstant(inProcessBus)
					.InSingletonScope();
			}
			else
				inProcessBus = Kernel.Get<InProcessBus<TAuthenticationToken>>();

			Bind<ICommandSender<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			Bind<ICommandPublisher<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			bool isHandlerRegistrationBound = Kernel.GetBindings(typeof(ICommandHandlerRegistrar)).Any();
			if (!isHandlerRegistrationBound)
			{
				Bind<ICommandHandlerRegistrar>()
					.ToConstant(inProcessBus)
					.InSingletonScope();
			}
		}
	}
}
