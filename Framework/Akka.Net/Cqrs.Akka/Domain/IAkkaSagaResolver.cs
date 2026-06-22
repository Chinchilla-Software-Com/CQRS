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
	/// A resolver for <see cref="IActorRef"/> instances of <see cref="ISaga{TAuthenticationToken}"/>.
	/// </summary>
	public interface IAkkaSagaResolver
	{
		/// <summary>
		/// Resolves instances of <typeparamref name="TSaga"/>.
		/// </summary>
		IActorRef ResolveActor<TSaga, TAuthenticationToken>(Guid rsn)
			where TSaga : ISaga<TAuthenticationToken>;

		/// <summary>
		/// Resolves instances of <typeparamref name="T"/>.
		/// </summary>
		IActorRef ResolveActor<T>();
	}
}