#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Actor;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Akka.Domain
{
	public abstract class AkkaAggregateRoot<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
		, IAggregateRoot<TAuthenticationToken>
	{
		private ICollection<IEvent<TAuthenticationToken>> Changes { get; set; }

		public Guid Id { get; protected set; }

		public int Version { get; protected set; }

		protected AkkaAggregateRoot()
		{
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		public IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		public virtual void MarkChangesAsCommitted()
		{
			Version = Version + Changes.Count;
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		public virtual void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			Type aggregateType = GetType();
			foreach (IEvent<TAuthenticationToken> @event in history.OrderBy(e =>e.Version))
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
	}
}