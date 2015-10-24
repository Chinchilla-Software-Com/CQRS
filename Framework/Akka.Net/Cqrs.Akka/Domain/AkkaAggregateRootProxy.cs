#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Akka.Actor;
using Cqrs.Akka.Domain.Commands;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	public abstract class AkkaAggregateRootProxy<TAuthenticationToken, TAggregateRoot>
		: IAkkaAggregateRootProxy<TAggregateRoot>
		, IAggregateRoot<TAuthenticationToken>
		// TODO think about if this is necessary again.
		// where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		public IActorRef ActorReference { get; internal set; }

		public abstract TAggregateRoot Aggregate { get; }

		#region Implementation of IAggregateRoot<TAuthenticationToken>

		public Guid Id
		{
			get { return ActorReference.Ask<Guid>(new GetAkkaAggregateRootId()).Result; }
		}

		public int Version
		{
			get { return ActorReference.Ask<int>(new GetAkkaAggregateRootVersion()).Result; }
		}

		public IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			throw new NotImplementedException();
		}

		public void MarkChangesAsCommitted()
		{
			throw new NotImplementedException();
		}

		public void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}