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
using System.Runtime.Serialization;
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
	[Serializable]
	public abstract class AggregateRoot<TAuthenticationToken> : IAggregateRoot<TAuthenticationToken>
	{
		private ReaderWriterLockSlim Lock { get; set; }

		private ICollection<IEvent<TAuthenticationToken>> Changes { get; set; }

		/// <summary>
		/// The identifier of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		public Guid Id { get; protected set; }

		/// <summary>
		/// The current version of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		public int Version { get; protected set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		protected AggregateRoot()
		{
			Lock = new ReaderWriterLockSlim();
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		/// <summary>
		/// Get all applied changes that haven't yet been committed.
		/// </summary>
		public IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		/// <summary>
		/// Mark all applied changes as committed, increment <see cref="Version"/> and flush the <see cref="Changes">internal collection of changes</see>.
		/// </summary>
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

		/// <summary>
		/// Apply all the <see cref="IEvent{TAuthenticationToken}">events</see> in <paramref name="history"/>
		/// using event replay to this instance.
		/// </summary>
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

		/// <summary>
		/// Call the "Apply" method with a signature matching the provided <paramref name="event"/> without using event replay to this instance.
		/// </summary>
		/// <remarks>
		/// This means a method named "Apply", with return type void and one parameter must exist to be applied.
		/// If no method exists, nothing is applied
		/// The parameter type must match exactly the <see cref="Type"/> of the provided <paramref name="event"/>.
		/// </remarks>
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
					Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(Changes.Concat(new[] { @event }).ToList());
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