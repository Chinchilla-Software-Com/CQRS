#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Ninject.InProcess.EventStore
{
	/// <summary>
	/// An <see cref="IEventStore{TAuthenticationToken}"/> that uses a local (non-static) <see cref="IDictionary{TKey,TValue}"/>.
	/// This does not manage memory in any way and will continue to grow. Mostly suitable for running tests or short lived processes.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use Cqrs.Events.InProcessEventStore<TAuthenticationToken> instead.")]
	public class InProcessEventStore<TAuthenticationToken> : Events.InProcessEventStore<TAuthenticationToken>
	{
	}
}