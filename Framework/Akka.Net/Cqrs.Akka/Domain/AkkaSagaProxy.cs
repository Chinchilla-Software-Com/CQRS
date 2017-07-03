#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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
	public class AkkaSagaProxy<TAuthenticationToken, TSaga>
		: IAkkaSagaProxy<TSaga>
		, ISaga<TAuthenticationToken>
		// TODO think about if this is necessary again.
		// where TSaga : ISaga<TAuthenticationToken>
	{
		public IActorRef ActorReference { get; internal set; }

		public TSaga Saga { get; protected set; }

		#region Implementation of ISaga<TAuthenticationToken>

		public virtual Guid Id
		{
			get { return ActorReference.Ask<Guid>(new GetAkkaSagaId()).Result; }
		}

		public virtual int Version
		{
			get { return ActorReference.Ask<int>(new GetAkkaSagaVersion()).Result; }
		}

		public virtual IEnumerable<ISagaEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return ((ISaga<TAuthenticationToken>)Saga).GetUncommittedChanges();
		}

		public virtual void MarkChangesAsCommitted()
		{
			((ISaga<TAuthenticationToken>)Saga).MarkChangesAsCommitted();
		}

		public virtual void LoadFromHistory(IEnumerable<ISagaEvent<TAuthenticationToken>> history)
		{
			((ISaga<TAuthenticationToken>)Saga).LoadFromHistory(history);
		}

		#endregion
	}
}