#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Bus;
using Cqrs.Commands;
using Ninject.Modules;

namespace Cqrs.Ninject.InProcess.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that configures the <see cref="InProcessBus{TAuthenticationToken}"/> as a <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use Cqrs.Ninject.Configuration.InProcessCommandBusModule<TAuthenticationToken> instead.")]
	public class InProcessCommandBusModule<TAuthenticationToken> : Ninject.Configuration.InProcessCommandBusModule<TAuthenticationToken>
	{
	}
}
