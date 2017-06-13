#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Cqrs.Domain;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaSagaResolver
	{
		IActorRef ResolveActor<TSaga, TAuthenticationToken>(Guid rsn)
			where TSaga : ISaga<TAuthenticationToken>;

		IActorRef ResolveActor<T>();
	}
}