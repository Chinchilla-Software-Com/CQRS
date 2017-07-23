#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Bus;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.InProcess.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that configures the <see cref="InProcessBus{TAuthenticationToken}"/> as a <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use Cqrs.Ninject.Configuration.InProcessEventBusModule<TAuthenticationToken> instead.")]
	public class InProcessEventBusModule<TAuthenticationToken> : Ninject.Configuration.InProcessEventBusModule<TAuthenticationToken>
	{
	}
}