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
using Cqrs.Events;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that configures the <see cref="InProcessBus{TAuthenticationToken}"/> as a <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class InProcessEventBusModule<TAuthenticationToken> : ResolvableModule
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
		/// Register the <see cref="IEventPublisher{TAuthenticationToken}"/>, <see cref="IEventReceiver{TAuthenticationToken}"/> and <see cref="IEventHandlerRegistrar"/>.
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			bool isInProcessBusBound = Kernel.GetBindings(typeof (InProcessBus<TAuthenticationToken>)).Any();
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

			Bind<IEventPublisher<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			Bind<IEventReceiver<TAuthenticationToken>>()
				.ToConstant(inProcessBus)
				.InSingletonScope();

			bool isHandlerRegistrationBound = Kernel.GetBindings(typeof(IEventHandlerRegistrar)).Any();
			if (!isHandlerRegistrationBound)
			{
#if NET40
				Bind<IEventHandlerRegistrar>()
#else
				Bind<IAsyncEventHandlerRegistrar>()
#endif
					.ToConstant(inProcessBus)
					.InSingletonScope();
			}
		}
	}
}
