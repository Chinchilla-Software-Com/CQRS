#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.Azure.Functions.ServiceBus.Configuration
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventBusReceiverModule<TAuthenticationToken> : ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load(IServiceCollection services)
		{
			bool isAzureBusHelper = IsRegistered<IAzureBusHelper<TAuthenticationToken>>(services);
			if (!isAzureBusHelper)
			{
				services.AddSingleton<IAzureBusHelper<TAuthenticationToken>, AzureBusHelper<TAuthenticationToken>>();
			}

			RegisterEventMessageSerialiser(services);
			var bus = GetOrCreateBus<AzureEventBusReceiver<TAuthenticationToken>>(services);

			RegisterEventReceiver(services, bus);
			RegisterEventHandlerRegistrar(services, bus);
		}

		#endregion

		/// <summary>
		/// Checks if an existing <typeparamref name="TBus"/> has already been registered, if not
		/// it tries to instantiates a new instance via resolution and registers that instance.
		/// </summary>
		/// <typeparam name="TBus">The <see cref="Type"/> of bus to resolve. Best if a class not an interface.</typeparam>
		public virtual TBus GetOrCreateBus<TBus>(IServiceCollection services)
			where TBus : class, IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			bool isBusBound = IsRegistered<TBus>(services);
			TBus bus;
			if (!isBusBound)
			{
				bus = Resolve<TBus>(services);
				services.AddSingleton<TBus>(bus);
			}
			else
				bus = Resolve<TBus>(services);

			return bus;
		}

		/// <summary>
		/// Register the CQRS event receiver
		/// </summary>
#if NET5_0_OR_GREATER
		public virtual void RegisterEventReceiver(IServiceCollection services, IEventReceiver<TAuthenticationToken> bus)
#else
		public virtual void RegisterEventReceiver<TBus>(IServiceCollection services, TBus bus)
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
#endif
		{
			services.AddSingleton<IEventReceiver<TAuthenticationToken>>(bus);
		}

		/// <summary>
		/// Register the CQRS event handler registrar
		/// </summary>
#if NET5_0_OR_GREATER
		public virtual void RegisterEventHandlerRegistrar(IServiceCollection services, IEventHandlerRegistrar bus)
#else
		public virtual void RegisterEventHandlerRegistrar<TBus>(IServiceCollection services, TBus bus)
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
#endif
		{
			bool isHandlerRegistrationBound = IsRegistered<IEventHandlerRegistrar>(services);
			if (!isHandlerRegistrationBound)
			{
				services.AddSingleton<IEventHandlerRegistrar>(bus);
			}
		}

		/// <summary>
		/// Register the CQRS event handler message serialiser
		/// </summary>
		public virtual void RegisterEventMessageSerialiser(IServiceCollection services)
		{
			bool isMessageSerialiserBound = IsRegistered<IMessageSerialiser<TAuthenticationToken>>(services);
			if (!isMessageSerialiserBound)
			{
				services.AddSingleton<IMessageSerialiser<TAuthenticationToken>, MessageSerialiser<TAuthenticationToken>>();
			}
		}
	}
}
