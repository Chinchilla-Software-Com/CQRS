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
	/// A remote proxy to an <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
	public class AkkaAggregateRootProxy<TAuthenticationToken, TAggregateRoot>
		: IAkkaAggregateRootProxy<TAggregateRoot>
		, IAggregateRoot<TAuthenticationToken>
		// TODO think about if this is necessary again.
		// where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the <see cref="IActorRef"/>.
		/// </summary>
		public IActorRef ActorReference { get; internal set; }

		/// <summary>
		/// Gets the <typeparamref name="TAggregateRoot"/>.
		/// </summary>
		public TAggregateRoot Aggregate { get; protected set; }

		#region Implementation of IAggregateRoot<TAuthenticationToken>

		/// <summary>
		/// The identifier of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		public virtual Guid Id
		{
			get { return ActorReference.Ask<Guid>(new GetAkkaAggregateRootId()).Result; }
		}

		/// <summary>
		/// The current version of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		public virtual int Version
		{
			get { return ActorReference.Ask<int>(new GetAkkaAggregateRootVersion()).Result; }
		}

		/// <summary>
		/// Get all applied changes that haven't yet been committed.
		/// </summary>
		public virtual IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return ((IAggregateRoot<TAuthenticationToken>)Aggregate).GetUncommittedChanges();
		}

		/// <summary>
		/// Mark all applied changes as committed, increment <see cref="Version"/> and flush the internal collection of changes.
		/// </summary>
		public virtual void MarkChangesAsCommitted()
		{
			((IAggregateRoot<TAuthenticationToken>)Aggregate).MarkChangesAsCommitted();
		}

		/// <summary>
		/// Apply all the <see cref="IEvent{TAuthenticationToken}">events</see> in <paramref name="history"/>
		/// using event replay to this instance.
		/// </summary>
		public virtual void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			((IAggregateRoot<TAuthenticationToken>)Aggregate).LoadFromHistory(history);
		}

		#endregion
	}
}