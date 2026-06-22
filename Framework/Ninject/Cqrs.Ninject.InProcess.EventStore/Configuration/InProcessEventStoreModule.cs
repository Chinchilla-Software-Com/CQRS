#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.InProcess.EventStore.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that configures the <see cref="InProcessEventStore{TAuthenticationToken}"/> as a <see cref="IEventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use Cqrs.Ninject.Configuration.InProcessEventStoreModule<TAuthenticationToken> instead.")]
	public class InProcessEventStoreModule<TAuthenticationToken> : Ninject.Configuration.InProcessEventStoreModule<TAuthenticationToken>
	{
	}
}