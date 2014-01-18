using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface IAggregateRoot<TAuthenticationToken>
	{
		Guid Id { get; }

		int Version { get; }

		IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges();

		void MarkChangesAsCommitted();

		void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history);
	}
}