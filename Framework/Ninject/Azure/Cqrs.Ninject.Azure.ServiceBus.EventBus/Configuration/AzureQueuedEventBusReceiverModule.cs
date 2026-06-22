#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ServiceBus;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureQueuedEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedEventBusReceiverModule<TAuthenticationToken>
		: AzureEventBusReceiverModule<TAuthenticationToken>
	{
		#region Overrides of AzureEventBusReceiverModule<TAuthenticationToken>

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

			RegisterEventMessageSerialiser();
			var bus = GetOrCreateBus<AzureQueuedEventBusReceiver<TAuthenticationToken>>();

			RegisterEventReceiver(bus);
			RegisterEventHandlerRegistrar(bus);
		}

		#endregion
	}
}