using System;
using System.Collections.Generic;
using System.Threading;
using Cqrs.Domain.Exception;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Domain
{
	public abstract class AggregateRoot<TPermissionScope> : IAggregateRoot<TPermissionScope>
	{
		private ReaderWriterLockSlim Lock { get; set; }

		private IList<IEvent<TPermissionScope>> Changes { get; set; }

		public Guid Id { get; protected set; }

		public int Version { get; protected set; }

		protected AggregateRoot()
		{
			Lock = new ReaderWriterLockSlim();
			Changes = new List<IEvent<TPermissionScope>>();
		}

		public IEnumerable<IEvent<TPermissionScope>> GetUncommittedChanges()
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

		public void LoadFromHistory(IEnumerable<IEvent<TPermissionScope>> history)
		{
			foreach (IEvent<TPermissionScope> @event in history)
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.Id);
				ApplyChange(@event, false);
			}
		}

		protected void ApplyChange(IEvent<TPermissionScope> @event)
		{
			ApplyChange(@event, true);
		}

		private void ApplyChange(IEvent<TPermissionScope> @event, bool isNew)
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