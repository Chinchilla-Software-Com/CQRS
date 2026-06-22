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
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureCommandBusPublisher{TAuthenticationToken}"/> as the <see cref="ICommandPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use AzureCommandBusPublisherModule")]
	public class AzureCommandBusSenderModule<TAuthenticationToken> : AzureCommandBusPublisherModule<TAuthenticationToken>
	{
	}
}