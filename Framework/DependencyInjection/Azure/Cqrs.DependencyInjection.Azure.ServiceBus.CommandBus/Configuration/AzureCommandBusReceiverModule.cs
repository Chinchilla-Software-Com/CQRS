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
using Cqrs.Commands;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Azure.ServiceBus.CommandBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureCommandBusReceiverModule<TAuthenticationToken>
		: AzureCommandBusReceiverModule<TAuthenticationToken, AzureCommandBusReceiver<TAuthenticationToken>>
	{
	}

	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TBus">The <see cref="Type"/> of bus to resolve. Best if a class not an interface.</typeparam>
	public class AzureCommandBusReceiverModule<TAuthenticationToken, TBus>
		: ResolvableModule
			where TBus : class,
#if NETSTANDARD2_0 || NET48_OR_GREATER
				IAsyncCommandReceiver<TAuthenticationToken>, IAsyncCommandHandlerRegistrar
#else
				ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
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

			RegisterCommandMessageSerialiser(services);
			var bus = GetOrCreateBus(services);

			RegisterCommandReceiver(services, bus);
			RegisterCommandHandlerRegistrar(services, bus);
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
		/// Register the CQRS command receiver
		/// </summary>
		public virtual void RegisterCommandReceiver(IServiceCollection services,
#if NETSTANDARD2_0 || NET48_OR_GREATER
			IAsyncCommandReceiver<TAuthenticationToken> bus
#else
			TBus bus
#endif
		)
		{
			services.AddSingleton<
#if NETSTANDARD2_0 || NET48_OR_GREATER
				IAsyncCommandReceiver
#else
				ICommandReceiver
#endif
				<TAuthenticationToken>>(bus);
		}

		/// <summary>
		/// Register the CQRS command handler registrar
		/// </summary>
		public virtual void RegisterCommandHandlerRegistrar(IServiceCollection services,
#if NETSTANDARD2_0 || NET48_OR_GREATER
			IAsyncCommandHandlerRegistrar bus
#else
			TBus bus
#endif
		)
		{
			services.AddSingleton<
#if NETSTANDARD2_0 || NET48_OR_GREATER
				IAsyncCommandHandlerRegistrar
#else
				ICommandHandlerRegistrar
#endif
				>(bus);
		}

		/// <summary>
		/// Register the CQRS command handler message serialiser
		/// </summary>
		public virtual void RegisterCommandMessageSerialiser(IServiceCollection services)
		{
			bool isMessageSerialiserBound = IsRegistered<IMessageSerialiser<TAuthenticationToken>>(services);
			if (!isMessageSerialiserBound)
			{
				services.AddSingleton<IMessageSerialiser<TAuthenticationToken>, MessageSerialiser<TAuthenticationToken>>();
			}
		}
	}
}