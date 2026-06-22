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

namespace Cqrs.DependencyInjection.Azure.ServiceBus.EventBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureQueuedEventBusReceiver{TAuthenticationToken}"/> as the <see cref="IEventReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedEventBusReceiverModule<TAuthenticationToken>
		: AzureEventBusReceiverModule<TAuthenticationToken, AzureQueuedEventBusReceiver<TAuthenticationToken>>
	{
	}
}