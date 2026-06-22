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

namespace Cqrs.Azure.EventHub.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureEventBusPublisher{TAuthenticationToken}"/> as the <see cref="IEventPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use AzureEventHubPublisherModule")]
	public class AzureEventPublisherModule<TAuthenticationToken> : AzureEventHubPublisherModule<TAuthenticationToken> 
	{
	}
}