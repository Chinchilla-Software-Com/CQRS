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
	public class AzureCommandBusReceiverModule<TAuthenticationToken> : ResolvableModule
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
			var bus = GetOrCreateBus<AzureCommandBusReceiver<TAuthenticationToken>>(services);

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
			where TBus : class, ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
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
		/// Register the CQRS command receiver
		/// </summary>
#if NET5_0_OR_GREATER
		public virtual void RegisterCommandReceiver(IServiceCollection services, ICommandReceiver<TAuthenticationToken> bus)
#else
		public virtual void RegisterCommandReceiver<TBus>(IServiceCollection services, TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			services.AddSingleton<ICommandReceiver<TAuthenticationToken>>(bus);
		}

		/// <summary>
		/// Register the CQRS command handler registrar
		/// </summary>
#if NET5_0_OR_GREATER
		public virtual void RegisterCommandHandlerRegistrar(IServiceCollection services, ICommandHandlerRegistrar bus)
#else
		public virtual void RegisterCommandHandlerRegistrar<TBus>(IServiceCollection services, TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			services.AddSingleton<ICommandHandlerRegistrar>(bus);
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
