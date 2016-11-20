#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using cdmdotnet.Logging;

namespace Cqrs.Events
{
	/// <summary>
	/// A, <see cref="EventStore{TAuthenticationToken}"/> that uses a <see cref="MemoryCache"/> implementation, flushing out data (I.E. it's not persisted)
	/// </summary>
	public class MemoryCacheEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		protected MemoryCache EventStoreByType { get; private set; }

		protected MemoryCache EventStoreByCorrelationId { get; private set; }

		public MemoryCacheEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			Guid id = Guid.NewGuid();
			EventStoreByType = new MemoryCache(string.Format("EventStoreByType-{0:N}", id));
			EventStoreByCorrelationId = new MemoryCache(string.Format("EventStoreByCorrelationId-{0:N}", id));
		}

		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			if (!EventStoreByType.Contains(streamName))
			{
				Logger.LogDebug(string.Format("The event store has no items '{0}'.", streamName));
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			CacheItem item = EventStoreByType.GetCacheItem(streamName);
			if (item == null)
			{
				Logger.LogDebug(string.Format("The event store had an item '{0}' but doesn't now.", streamName));
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			var events = item.Value as IEnumerable<EventData>;
			if (events == null)
			{
				if (item.Value == null)
					Logger.LogDebug(string.Format("The event store had an item '{0}' but it was null.", streamName));
				else
					Logger.LogWarning(string.Format("The event store had an item '{0}' but it was of type {1}.", streamName, item.Value.GetType()));
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}
			IEnumerable<EventData> query = events
				.Where(eventData => eventData.Version > fromVersion)
				.OrderByDescending(eventData => eventData.Version);

			if (useLastEventOnly)
				query = query.AsQueryable().Take(1);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			if (!EventStoreByCorrelationId.Contains(correlationId.ToString("N")))
			{
				Logger.LogDebug(string.Format("The event store has no items by the correlationId '{0:N}'.", correlationId));
				return Enumerable.Empty<EventData>();
			}

			CacheItem item = EventStoreByCorrelationId.GetCacheItem(correlationId.ToString("N"));
			if (item == null)
			{
				Logger.LogDebug(string.Format("The event store had some items by the correlationId '{0:N}' but doesn't now.", correlationId));
				return Enumerable.Empty<EventData>();
			}

			var events = item.Value as IEnumerable<EventData>;
			if (events == null)
			{
				if (item.Value == null)
					Logger.LogDebug(string.Format("The event store had some items by the correlationId '{0:N}' but it was null.", correlationId));
				else
					Logger.LogWarning(string.Format("The event store had some items by the correlationId '{0:N}' but it was of type {1}.", correlationId, item.Value.GetType()));
				return Enumerable.Empty<EventData>();
			}
			IEnumerable<EventData> query = events.OrderBy(eventData => eventData.Timestamp);

			return query.ToList();
		}

		protected override void PersistEvent(EventData eventData)
		{
			IList<EventData> events = new List<EventData>();

			// By correlationId first
			Guid correlationId = eventData.CorrelationId;
			object item = EventStoreByCorrelationId.AddOrGetExisting(correlationId.ToString("N"), events, GetDetaultCacheItemPolicy());
			if (item != null)
			{
				events = item as IList<EventData>;
				if (events == null)
				{
					Logger.LogWarning(string.Format("The event store had some items by the correlationId '{0:N}' but it doesn't now.", correlationId));
					throw new Exception(string.Format("The event store had some items by the correlationId '{0:N}' but it doesn't now.", correlationId));
				}
			}

			events.Add(eventData);
			// Reset the variable for it's next usage
			events = new List<EventData>();

			// By type next
			string streamName = eventData.AggregateId;
			item = EventStoreByType.AddOrGetExisting(streamName, events, GetDetaultCacheItemPolicy());
			if (item != null)
			{
				events = item as IList<EventData>;
				if (events == null)
				{
					Logger.LogWarning(string.Format("The event store had an item by id '{0}' but it doesn't now.", streamName));
					throw new Exception(string.Format("The event store had an item by id '{0}' but it doesn't now.", streamName));
				}
			}

			events.Add(eventData);
		}

		#endregion

		/// <summary>
		/// Get's a <see cref="CacheItemPolicy"/> with the <see cref="CacheItemPolicy.SlidingExpiration"/> set to 15 minutes
		/// </summary>
		protected virtual CacheItemPolicy GetDetaultCacheItemPolicy()
		{
			return new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 15, 0) };
		}
	}
}