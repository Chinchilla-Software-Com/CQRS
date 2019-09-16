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
	/// A resolver for <see cref="IActorRef"/> instances of <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// </summary>
	public interface IAkkaAggregateResolver
	{
		/// <summary>
		/// Resolves instances of <typeparamref name="TAggregate"/>.
		/// </summary>
		IActorRef ResolveActor<TAggregate, TAuthenticationToken>(Guid rsn)
			where TAggregate : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Resolves instances of <typeparamref name="T"/>.
		/// </summary>
		IActorRef ResolveActor<T>();
	}
}