#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Bus;
using Cqrs.Commands;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that configures the <see cref="InProcessBus{TAuthenticationToken}"/> as a <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class InProcessCommandBusModule<TAuthenticationToken> : ResolvableModule
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
		/// Register the <see cref="ICommandPublisher{TAuthenticationToken}"/>, <see cref="IPublishAndWaitCommandPublisher{TAuthenticationToken}"/>, <see cref="ICommandReceiver{TAuthenticationToken}"/> and <see cref="ICommandHandlerRegistrar"/>
		/// Register (for backwards compatibility) <see cref="ICommandSender{TAuthenticationToken}"/>
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

			Bind<IPublishAndWaitCommandPublisher<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			Bind<ICommandReceiver<TAuthenticationToken>>()
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
