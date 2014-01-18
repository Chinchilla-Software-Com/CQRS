using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IAggregateRoot<TPermissionToken>
	{
		Guid Id { get; }

		int Version { get; }

		IEnumerable<IEvent<TPermissionToken>> GetUncommittedChanges();

		void MarkChangesAsCommitted();

		void LoadFromHistory(IEnumerable<IEvent<TPermissionToken>> history);
	}
}