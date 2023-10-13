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

namespace Cqrs.DependencyInjection.Azure.ServiceBus.CommandBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureQueuedCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedCommandBusReceiverModule<TAuthenticationToken>
		: AzureCommandBusReceiverModule<TAuthenticationToken, AzureQueuedCommandBusReceiver<TAuthenticationToken>>
	{
	}
}