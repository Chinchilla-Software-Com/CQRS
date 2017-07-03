#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Domain
{
	/// <summary>
	/// A larger unit of encapsulation than just a class. Every transaction is scoped to a single aggregate. The lifetimes of the components of an aggregate are bounded by the lifetime of the entire aggregate.
	/// 
	/// Concretely, an aggregate will handle commands, apply events, and have a state model encapsulated within it that allows it to implement the required command validation, thus upholding the invariants (business rules) of the aggregate.
	/// </summary>
	/// <remarks>
	/// Why is the use of GUID as IDs a good practice?
	/// 
	/// Because they are (reasonably) globally unique, and can be generated either by the server or by the client.
	/// </remarks>
	public abstract class AggregateRoot<TAuthenticationToken> : IAggregateRoot<TAuthenticationToken>
	{
		private ReaderWriterLockSlim Lock { get; set; }

		private ICollection<IEvent<TAuthenticationToken>> Changes { get; set; }

		public Guid Id { get; protected set; }

		public int Version { get; protected set; }

		protected AggregateRoot()
		{
			Lock = new ReaderWriterLockSlim();
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		public IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		public virtual void MarkChangesAsCommitted()
		{
			Lock.EnterWriteLock();
			try
			{
				Version = Version + Changes.Count;
				Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		public virtual void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			Type aggregateType = GetType();
			foreach (IEvent<TAuthenticationToken> @event in history.OrderBy(e => e.Version))
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.Id, aggregateType, Version + 1, @event.Version);
				ApplyChange(@event, true);
			}
		}

		protected virtual void ApplyChange(IEvent<TAuthenticationToken> @event)
		{
			ApplyChange(@event, false);
		}

		private void ApplyChange(IEvent<TAuthenticationToken> @event, bool isEventReplay)
		{
			Lock.EnterWriteLock();
			try
			{
				this.AsDynamic().Apply(@event);
				if (!isEventReplay)
				{
					Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new []{@event}.Concat(Changes).ToList());
				}
				else
				{
					Id = @event.Id;
					Version++;
				}
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}
	}
}