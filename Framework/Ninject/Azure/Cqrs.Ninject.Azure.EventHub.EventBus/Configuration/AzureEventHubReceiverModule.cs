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
using Ninject.Modules;

namespace Cqrs.Azure.EventHub.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventHubReceiverModule<TAuthenticationToken> : NinjectModule
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

			RegisterEventHandlerRegistrar();
			RegisterEventMessageSerialiser();
		}

		#endregion

		/// <summary>
		/// Register the CQRS event handler registrar
		/// </summary>
		public virtual void RegisterEventHandlerRegistrar()
		{
			Bind
			<
#if NET452
				IEventHandlerRegistrar
#else
				IAsyncEventHandlerRegistrar
#endif
			>()
				.To<AzureEventBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
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
