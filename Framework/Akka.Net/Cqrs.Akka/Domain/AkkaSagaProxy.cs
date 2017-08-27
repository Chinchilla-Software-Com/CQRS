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
	/// <summary>
	/// A remote proxy to an <see cref="ISaga{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
	public class AkkaSagaProxy<TAuthenticationToken, TSaga>
		: IAkkaSagaProxy<TSaga>
		, ISaga<TAuthenticationToken>
		// TODO think about if this is necessary again.
		// where TSaga : ISaga<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the <see cref="IActorRef"/>.
		/// </summary>
		public IActorRef ActorReference { get; internal set; }

		/// <summary>
		/// Gets the <typeparamref name="TSaga"/>.
		/// </summary>
		public TSaga Saga { get; protected set; }

		#region Implementation of ISaga<TAuthenticationToken>

		/// <summary>
		/// The identifier of the <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		public virtual Guid Id
		{
			get { return ActorReference.Ask<Guid>(new GetAkkaSagaId()).Result; }
		}

		/// <summary>
		/// The current version of this <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		public virtual int Version
		{
			get { return ActorReference.Ask<int>(new GetAkkaSagaVersion()).Result; }
		}

		/// <summary>
		/// Get all applied changes that haven't yet been committed.
		/// </summary>
		public virtual IEnumerable<ISagaEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return ((ISaga<TAuthenticationToken>)Saga).GetUncommittedChanges();
		}

		/// <summary>
		/// Mark all applied changes as committed, increment <see cref="Version"/> and flush the internal collection of changes.
		/// </summary>
		public virtual void MarkChangesAsCommitted()
		{
			((ISaga<TAuthenticationToken>)Saga).MarkChangesAsCommitted();
		}

		/// <summary>
		/// Apply all the <see cref="IEvent{TAuthenticationToken}">events</see> in <paramref name="history"/>
		/// using event replay to this instance.
		/// </summary>
		public virtual void LoadFromHistory(IEnumerable<ISagaEvent<TAuthenticationToken>> history)
		{
			((ISaga<TAuthenticationToken>)Saga).LoadFromHistory(history);
		}

		#endregion
	}
}