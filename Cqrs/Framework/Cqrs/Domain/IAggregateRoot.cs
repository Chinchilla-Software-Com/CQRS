using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IAggregateRoot<TPermissionScope>
	{
		Guid Id { get; }

		int Version { get; }

		IEnumerable<IEvent<TPermissionScope>> GetUncommittedChanges();

		void MarkChangesAsCommitted();

		void LoadFromHistory(IEnumerable<IEvent<TPermissionScope>> history);
	}
}