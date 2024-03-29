﻿#region Copyright
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

namespace Cqrs.DependencyInjection.Azure.ServiceBus.EventBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventBusReceiverModule<TAuthenticationToken>
		: AzureEventBusReceiverModule<TAuthenticationToken, AzureEventBusReceiver<TAuthenticationToken>>
	{
	}

	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TBus">The <see cref="Type"/> of bus to resolve. Best if a class not an interface.</typeparam>
	public class AzureEventBusReceiverModule<TAuthenticationToken, TBus>
		: ResolvableModule
			where TBus : class,
#if NETSTANDARD2_0 || NET48_OR_GREATER
				IAsyncEventReceiver<TAuthenticationToken>, IAsyncEventHandlerRegistrar
#else
				IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
#endif
	{
		#region Overrides of ResolvableModule

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
			var bus = GetOrCreateBus(services);

			RegisterEventReceiver(services, bus);
			RegisterEventHandlerRegistrar(services, bus);
		}

		#endregion

		/// <summary>
		/// Checks if an existing <typeparamref name="TBus"/> has already been registered, if not
		/// it tries to instantiates a new instance via resolution and registers that instance.
		/// </summary>
		public virtual TBus GetOrCreateBus(IServiceCollection services)
		{
			bool isBusBound = IsRegistered<TBus>(services);
			TBus bus;
			if (!isBusBound)
			{
				services.AddSingleton<TBus, TBus>();
				bus = Resolve<TBus>(services);
				Unbind<TBus>(services);
				services.AddSingleton<TBus>(bus);
			}
			else
				bus = Resolve<TBus>(services);

			return bus;
		}

		/// <summary>
		/// Register the CQRS event receiver
		/// </summary>
		public virtual void RegisterEventReceiver(IServiceCollection services,
#if NETSTANDARD2_0 || NET48_OR_GREATER
			IAsyncEventReceiver<TAuthenticationToken> bus
#else
			TBus bus
#endif
		)
		{
			services.AddSingleton<
#if NETSTANDARD2_0 || NET48_OR_GREATER
				IAsyncEventReceiver
#else
				IEventReceiver
#endif
				<TAuthenticationToken>>(bus);
		}

		/// <summary>
		/// Register the CQRS event handler registrar
		/// </summary>
		public virtual void RegisterEventHandlerRegistrar(IServiceCollection services,
#if NETSTANDARD2_0 || NET48_OR_GREATER
			IAsyncEventHandlerRegistrar bus
#else
			TBus bus
#endif
		)
		{
			bool isHandlerRegistrationBound = IsRegistered<IEventHandlerRegistrar>(services);
			if (!isHandlerRegistrationBound)
			{
				services.AddSingleton<
#if NETSTANDARD2_0 || NET48_OR_GREATER
					IAsyncEventHandlerRegistrar
#else
					IEventHandlerRegistrar 
#endif
					>(bus);
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