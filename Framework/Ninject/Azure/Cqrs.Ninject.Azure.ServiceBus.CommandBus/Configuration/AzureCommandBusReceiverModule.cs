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
using Cqrs.Commands;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureCommandBusReceiverModule<TAuthenticationToken> : NinjectModule
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
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
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
		/// Register the CQRS command receiver
		/// </summary>
		public virtual void RegisterCommandReceiver<TBus>(TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
		{
			Bind<ICommandReceiver<TAuthenticationToken>>()
				.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS command handler registrar
		/// </summary>
		public virtual void RegisterCommandHandlerRegistrar<TBus>(TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
		{
			Bind<ICommandHandlerRegistrar>()
					.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS command handler message serialiser
		/// </summary>
		public virtual void RegisterCommandMessageSerialiser()
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
