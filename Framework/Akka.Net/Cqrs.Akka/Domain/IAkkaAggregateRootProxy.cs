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
	/// A remote proxy to an <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAggregate">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
	public interface IAkkaAggregateRootProxy<out TAggregate>
	{
		/// <summary>
		/// Gets the <see cref="IActorRef"/>.
		/// </summary>
		IActorRef ActorReference { get; }

		/// <summary>
		/// Gets the <typeparamref name="TAggregate"/>.
		/// </summary>
		TAggregate Aggregate { get; }
	}
}