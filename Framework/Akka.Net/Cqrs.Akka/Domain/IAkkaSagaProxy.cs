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
	public interface IAkkaSagaProxy<out TSaga>
	{
		IActorRef ActorReference { get; }

		TSaga Saga { get; }
	}
}