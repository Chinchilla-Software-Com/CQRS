#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Commands;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
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
		/// Register the Cqrs command receiver
		/// </summary>
		public virtual void RegisterCommandReceiver<TBus>(TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
		{
			Bind<ICommandReceiver<TAuthenticationToken>>()
				.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the Cqrs command handler registrar
		/// </summary>
		public virtual void RegisterCommandHandlerRegistrar<TBus>(TBus bus)
			where TBus : ICommandReceiver<TAuthenticationToken>, ICommandHandlerRegistrar
		{
			Bind<ICommandHandlerRegistrar>()
					.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the Cqrs command handler message serialiser
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
