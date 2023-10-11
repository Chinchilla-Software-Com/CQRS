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
using Cqrs.Ninject.Configuration;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureCommandBusReceiverModule<TAuthenticationToken>
		: ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			bool isAzureBusHelper = IsRegistered<IAzureBusHelper<TAuthenticationToken>>();
			if (!isAzureBusHelper)
			{
				Bind<IAzureBusHelper<TAuthenticationToken>>()
					.To<AzureBusHelper<TAuthenticationToken>>()
					.InSingletonScope();
			}

			RegisterCommandMessageSerialiser();
			var bus = GetOrCreateBus<AzureCommandBusReceiver<TAuthenticationToken>>();

			RegisterCommandReceiver(bus);
			RegisterCommandHandlerRegistrar(bus);
		}

		#endregion

		/// <summary>
		/// Checks if an existing <typeparamref name="TBus"/> has already been registered, if not
		/// it tries to instantiates a new instance via resolution and registers that instance.
		/// </summary>
		/// <typeparam name="TBus">The <see cref="Type"/> of bus to resolve. Best if a class not an interface.</typeparam>
		public virtual TBus GetOrCreateBus<TBus>()
			where TBus : class,
#if NETSTANDARD || NET6_0
				IAsyncCommandReceiver<TAuthenticationToken>, IAsyncCommandHandlerRegistrar
#else
				ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			bool isBusBound = IsRegistered<TBus>();
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
		/// Register the CQRS command receiver
		/// </summary>
#if NETSTANDARD || NET6_0
		public virtual void RegisterCommandReceiver(IAsyncCommandReceiver<TAuthenticationToken> bus)
#else
		public virtual void RegisterCommandReceiver<TBus>(TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			Bind<
#if NETSTANDARD || NET6_0
				IAsyncCommandReceiver
#else
				ICommandReceiver
#endif
				<TAuthenticationToken>>()
				.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS command handler registrar
		/// </summary>
#if NETSTANDARD || NET6_0
		public virtual void RegisterCommandHandlerRegistrar(IAsyncCommandHandlerRegistrar bus)
#else
		public virtual void RegisterCommandHandlerRegistrar<TBus>(TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
#endif
		{
			Bind<
#if NETSTANDARD || NET6_0
				IAsyncCommandHandlerRegistrar
#else
				ICommandHandlerRegistrar
#endif
				>()
				.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS command handler message serialiser
		/// </summary>
		public virtual void RegisterCommandMessageSerialiser()
		{
			bool isMessageSerialiserBound = IsRegistered<IMessageSerialiser<TAuthenticationToken>>();
			if (!isMessageSerialiserBound)
			{
				Bind<IMessageSerialiser<TAuthenticationToken>>()
					.To<MessageSerialiser<TAuthenticationToken>>()
					.InSingletonScope();
			}
		}
	}
}
