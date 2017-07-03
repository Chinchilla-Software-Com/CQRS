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
	public interface IAkkaAggregateResolver
	{
		IActorRef ResolveActor<TAggregate, TAuthenticationToken>(Guid rsn)
			where TAggregate : IAggregateRoot<TAuthenticationToken>;

		IActorRef ResolveActor<T>();
	}
}