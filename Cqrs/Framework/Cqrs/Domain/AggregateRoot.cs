using System;
using System.Collections.Generic;
using System.Threading;
using Cqrs.Domain.Exception;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Domain
{
	public abstract class AggregateRoot<TPermissionToken> : IAggregateRoot<TPermissionToken>
	{
		private ReaderWriterLockSlim Lock { get; set; }

		private IList<IEvent<TPermissionToken>> Changes { get; set; }

		public Guid Id { get; protected set; }

		public int Version { get; protected set; }

		protected AggregateRoot()
		{
			Lock = new ReaderWriterLockSlim();
			Changes = new List<IEvent<TPermissionToken>>();
		}

		public IEnumerable<IEvent<TPermissionToken>> GetUncommittedChanges()
		{
			Lock.EnterReadLock();
			try
			{
				return Changes;
			}
			finally
			{
				Lock.ExitReadLock();
			}
		}

		public void MarkChangesAsCommitted()
		{
			Lock.EnterWriteLock();
			try
			{
				Version = Version + Changes.Count;
				Changes.Clear();
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		public void LoadFromHistory(IEnumerable<IEvent<TPermissionToken>> history)
		{
			foreach (IEvent<TPermissionToken> @event in history)
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.Id);
				ApplyChange(@event, false);
			}
		}

		protected void ApplyChange(IEvent<TPermissionToken> @event)
		{
			ApplyChange(@event, true);
		}

		private void ApplyChange(IEvent<TPermissionToken> @event, bool isNew)
		{
			Lock.EnterWriteLock();
			try
			{
				this.AsDynamic().Apply(@event);
				if (isNew)
				{
					Changes.Add(@event);
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