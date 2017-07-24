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
using Ninject.Modules;

namespace Cqrs.Azure.EventHub.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureQueuedCommandBusReceiver{TAuthenticationToken}"/> as the <see cref="ICommandReceiver"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedCommandHubReceiverModule<TAuthenticationToken> : AzureCommandHubReceiverModule<TAuthenticationToken>
	{
		/// <summary>
		/// Register the CQRS command handler registrar
		/// </summary>
		public override void RegisterCommandHandlerRegistrar()
		{
			Bind<ICommandHandlerRegistrar>()
				.To<AzureQueuedCommandBusReceiver<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}