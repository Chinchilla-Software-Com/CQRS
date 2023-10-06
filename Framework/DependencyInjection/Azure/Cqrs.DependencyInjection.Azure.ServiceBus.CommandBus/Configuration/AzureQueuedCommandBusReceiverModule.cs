#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ServiceBus;
using Cqrs.Commands;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Azure.ServiceBus.CommandBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureQueuedCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedCommandBusReceiverModule<TAuthenticationToken>
		: AzureCommandBusReceiverModule<TAuthenticationToken>
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

			var bus = GetOrCreateBus<AzureQueuedCommandBusReceiver<TAuthenticationToken>>(services);

			RegisterCommandReceiver(services, bus);
			RegisterCommandHandlerRegistrar(services, bus);
			RegisterCommandMessageSerialiser(services);
		}

		#endregion
	}
}