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
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Azure.EventHub.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureQueuedEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedEventHubReceiverModule<TAuthenticationToken> : AzureEventHubReceiverModule<TAuthenticationToken>
	{
		/// <summary>
		/// Register the CQRS event handler registrar
		/// </summary>
		public override void RegisterEventHandlerRegistrar()
		{
			Bind
			<
#if NET452
				IEventHandlerRegistrar
#else
				IAsyncEventHandlerRegistrar
#endif
			>()
				.To<AzureQueuedEventBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}