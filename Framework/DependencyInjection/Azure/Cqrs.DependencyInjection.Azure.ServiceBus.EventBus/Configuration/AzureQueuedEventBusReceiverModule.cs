#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ServiceBus;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Azure.ServiceBus.EventBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureQueuedEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedEventBusReceiverModule<TAuthenticationToken>
		: AzureEventBusReceiverModule<TAuthenticationToken>
	{
		#region Overrides of AzureEventBusReceiverModule<TAuthenticationToken>

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load(IServiceCollection services)
		{
			bool isAzureServiceBusBound = IsRegistered<IAzureBusHelper<TAuthenticationToken>>(services);
			if (!isAzureServiceBusBound)
			{
				services.AddSingleton<IAzureBusHelper<TAuthenticationToken>, AzureBusHelper<TAuthenticationToken>>();
			}

			RegisterEventMessageSerialiser(services);
			var bus = GetOrCreateBus<AzureQueuedEventBusReceiver<TAuthenticationToken>>(services);

			RegisterEventReceiver(services, bus);
			RegisterEventHandlerRegistrar(services, bus);
		}

		#endregion
	}
}