#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Events;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventBusReceiverModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

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

			RegisterEventMessageSerialiser();
			var bus = GetOrCreateBus<AzureEventBusReceiver<TAuthenticationToken>>();

			RegisterEventReceiver(bus);
			RegisterEventHandlerRegistrar(bus);
		}

		#endregion

		/// <summary>
		/// Checks if an existing <typeparamref name="TBus"/> has already been registered, if not
		/// it tries to instantiates a new instance via resolution and registers that instance.
		/// </summary>
		/// <typeparam name="TBus">The <see cref="Type"/> of bus to resolve. Best if a class not an interface.</typeparam>
		public virtual TBus GetOrCreateBus<TBus>()
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			bool isBusBound = Kernel.GetBindings(typeof(TBus)).Any();
			TBus bus;
			if (!isBusBound)
			{
				bus = Kernel.Get<TBus>();
				Bind<TBus>()
					.ToConstant(bus)
					.InSingletonScope();
			}
			else
				bus = Kernel.Get<TBus>();

			return bus;
		}

		/// <summary>
		/// Register the CQRS event receiver
		/// </summary>
		public virtual void RegisterEventReceiver<TBus>(TBus bus)
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			Bind<IEventReceiver<TAuthenticationToken>>()
				.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS event handler registrar
		/// </summary>
		public virtual void RegisterEventHandlerRegistrar<TBus>(TBus bus)
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			bool isHandlerRegistrationBound = Kernel.GetBindings(typeof(IEventHandlerRegistrar)).Any();
			if (!isHandlerRegistrationBound)
			{
				Bind<IEventHandlerRegistrar>()
					.ToConstant(bus)
					.InSingletonScope();
			}
		}

		/// <summary>
		/// Register the CQRS event handler message serialiser
		/// </summary>
		public virtual void RegisterEventMessageSerialiser()
		{
			bool isMessageSerialiserBound = Kernel.GetBindings(typeof(IMessageSerialiser<TAuthenticationToken>)).Any();
			if (!isMessageSerialiserBound)
			{
				Bind<IMessageSerialiser<TAuthenticationToken>>()
					.To<MessageSerialiser<TAuthenticationToken>>()
					.InSingletonScope();
			}
		}
	}
}
