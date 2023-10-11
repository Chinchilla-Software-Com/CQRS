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

namespace Cqrs.Azure.Functions.ServiceBus.Configuration
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureFunctionCommandBusReceiverModule<TAuthenticationToken>
		: ResolvableModule
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

			RegisterCommandMessageSerialiser(services);
			var bus = GetOrCreateBus<AzureFunctionCommandBusReceiver<TAuthenticationToken>>(services);

			RegisterCommandReceiver(services, bus);
			RegisterCommandHandlerRegistrar(services, bus);
		}

		#endregion

		/// <summary>
		/// Checks if an existing <typeparamref name="TBus"/> has already been registered, if not
		/// it tries to instantiates a new instance via resolution and registers that instance.
		/// </summary>
		/// <typeparam name="TBus">The <see cref="Type"/> of bus to resolve. Best if a class not an interface.</typeparam>
		public virtual TBus GetOrCreateBus<TBus>(IServiceCollection services)
			where TBus : class,
#if NETSTANDARD
				IAsyncCommandReceiver<TAuthenticationToken>, IAsyncCommandHandlerRegistrar
#else
				ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
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
#if NETSTANDARD
		public virtual void RegisterCommandReceiver(IServiceCollection services, IAsyncCommandReceiver<TAuthenticationToken> bus)
#else
		public virtual void RegisterCommandReceiver<TBus>(IServiceCollection services, TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			services.AddSingleton<
#if NETSTANDARD
				IAsyncCommandReceiver
#else
				ICommandReceiver
#endif
				<TAuthenticationToken>>(bus);
		}

		/// <summary>
		/// Register the CQRS command handler registrar
		/// </summary>
#if NETSTANDARD
		public virtual void RegisterCommandHandlerRegistrar(IServiceCollection services, IAsyncCommandHandlerRegistrar bus)
#else
		public virtual void RegisterCommandHandlerRegistrar<TBus>(IServiceCollection services, TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			services.AddSingleton <
#if NETSTANDARD
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
