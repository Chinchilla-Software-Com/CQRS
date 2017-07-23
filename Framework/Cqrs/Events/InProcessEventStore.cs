#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEventStore{TAuthenticationToken}"/> that uses a local (non-static) <see cref="IDictionary{TKey,TValue}"/>.
	/// This does not manage memory in any way and will continue to grow. Mostly suitable for running tests or short lived processes.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class InProcessEventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> InMemoryDb { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="InProcessEventStore{TAuthenticationToken}"/> class.
		/// </summary>
		public InProcessEventStore()
		{
			InMemoryDb = new Dictionary<Guid, IList<IEvent<TAuthenticationToken>>>();
		}

		public void Save(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			IList<IEvent<TAuthenticationToken>> list;
			InMemoryDb.TryGetValue(@event.Id, out list);
			if (list == null)
			{
				list = new List<IEvent<TAuthenticationToken>>();
				InMemoryDb.Add(@event.Id, list);
			}
			list.Add(@event);
		}

		public IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			IList<IEvent<TAuthenticationToken>> events;
			InMemoryDb.TryGetValue(aggregateId, out events);
			return events != null
				? events.Where(x => x.Version > fromVersion)
				: new List<IEvent<TAuthenticationToken>>();
		}

		public IEnumerable<EventData> Get(Guid correlationId)
		{
			return Enumerable.Empty<EventData>();
		}

		public void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(typeof(T), @event);
		}
	}
}