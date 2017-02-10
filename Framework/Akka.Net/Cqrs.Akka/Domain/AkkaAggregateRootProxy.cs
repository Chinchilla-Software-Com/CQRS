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
	public class AkkaAggregateRootProxy<TAuthenticationToken, TAggregateRoot>
		: IAkkaAggregateRootProxy<TAggregateRoot>
		, IAggregateRoot<TAuthenticationToken>
		// TODO think about if this is necessary again.
		// where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		public IActorRef ActorReference { get; internal set; }

		public TAggregateRoot Aggregate { get; protected set; }

		#region Implementation of IAggregateRoot<TAuthenticationToken>

		public virtual Guid Id
		{
			get { return ActorReference.Ask<Guid>(new GetAkkaAggregateRootId()).Result; }
		}

		public virtual int Version
		{
			get { return ActorReference.Ask<int>(new GetAkkaAggregateRootVersion()).Result; }
		}

		public virtual IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return ((IAggregateRoot<TAuthenticationToken>)Aggregate).GetUncommittedChanges();
		}

		public virtual void MarkChangesAsCommitted()
		{
			((IAggregateRoot<TAuthenticationToken>)Aggregate).MarkChangesAsCommitted();
		}

		public virtual void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			((IAggregateRoot<TAuthenticationToken>)Aggregate).LoadFromHistory(history);
		}

		#endregion
	}
}