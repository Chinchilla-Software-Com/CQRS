#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public interface IEventStore<TAuthenticationToken>
	{
		void Save<T>(IEvent<TAuthenticationToken> @event);

		void Save(Type aggregateRootType, IEvent<TAuthenticationToken> @event);

		IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		IEnumerable<EventData> Get(Guid correlationId);
	}
}