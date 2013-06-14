using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IAggregateRoot
	{
		Guid Id { get; }
		int Version { get; }
		IEnumerable<IEvent> GetUncommittedChanges();
		void MarkChangesAsCommitted();
		void LoadFromHistory(IEnumerable<IEvent> history);
	}
}