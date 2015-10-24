#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaAggregateResolver
	{
		IActorRef Resolve<TAggregate>(Guid rsn);
	}
}