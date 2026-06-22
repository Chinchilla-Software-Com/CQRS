#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Cqrs.Domain;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// A remote proxy to an <see cref="ISaga{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
	public interface IAkkaSagaProxy<out TSaga>
	{
		/// <summary>
		/// Gets the <see cref="IActorRef"/>.
		/// </summary>
		IActorRef ActorReference { get; }

		/// <summary>
		/// Gets the <typeparamref name="TSaga"/>.
		/// </summary>
		TSaga Saga { get; }
	}
}