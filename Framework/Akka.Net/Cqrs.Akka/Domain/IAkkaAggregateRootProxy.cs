#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaAggregateRootProxy<out TAggregate>
	{
		IActorRef ActorReference { get; }

		TAggregate Aggregate { get; }
	}
}