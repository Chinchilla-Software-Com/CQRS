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
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// A, <see cref="EventStore{TAuthenticationToken}"/> that uses a <see cref="MemoryCache"/> implementation, flushing out data (I.E. it's not persisted)
	/// </summary>
	public class MemoryCacheEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="MemoryCache"/> of data grouped by event <see cref="Type"/>.
		/// </summary>
		protected MemoryCache EventStoreByType { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="MemoryCache"/> of data grouped by event <see cref="IMessage.CorrelationId"/>.
		/// </summary>
		protected MemoryCache EventStoreByCorrelationId { get; private set; }

		/// <summary>
		/// Gets of sets the SlidingExpirationValue, the value of "Cqrs.EventStore.MemoryCacheEventStore.SlidingExpiration" from <see cref="ConfigurationManager"/>.
		/// </summary>
		protected string SlidingExpirationValue { get; set; }

		/// <summary>
		/// Gets of sets the SlidingExpiration
		/// </summary>
		protected TimeSpan SlidingExpiration { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="MemoryCacheEventStore{TAuthenticationToken}"/> and calls <see cref="StartRefreshSlidingExpirationValue"/>.
		/// </summary>
		public MemoryCacheEventStore(IConfigurationManager configurationManager, IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			Guid id = Guid.NewGuid();
			ConfigurationManager = configurationManager;
			EventStoreByType = new MemoryCache(string.Format("EventStoreByType-{0:N}", id));
			EventStoreByCorrelationId = new MemoryCache(string.Format("EventStoreByCorrelationId-{0:N}", id));

			StartRefreshSlidingExpirationValue();
		}

		#region Overrides of EventStore<TAuthenticationToken>

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
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

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToVersion(Type aggregateRootType, Guid aggregateId, int version)
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
				.Where(eventData => eventData.Version <= version)
				.OrderByDescending(eventData => eventData.Version);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToDate(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
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
				.Where(eventData => eventData.Timestamp <= versionedDate)
				.OrderByDescending(eventData => eventData.Version);

			return query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
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

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
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
		/// Reads "Cqrs.EventStore.MemoryCacheEventStore.SlidingExpiration" from <see cref="ConfigurationManager"/>, checks if it has changed and then
		/// Update <see cref="SlidingExpiration"/> with the new value.
		/// </summary>
		protected virtual void RefreshSlidingExpirationValue()
		{
			// First refresh the EventBlackListProcessing property
			string slidingExpirationValue;
			if (!ConfigurationManager.TryGetSetting("Cqrs.EventStore.MemoryCacheEventStore.SlidingExpiration", out slidingExpirationValue))
				slidingExpirationValue = "0, 15, 0";

			if (SlidingExpirationValue != slidingExpirationValue)
			{
				string[] slidingExpirationParts = slidingExpirationValue.Split(',');
				if (slidingExpirationParts.Length != 3 || slidingExpirationParts.Length != 4)
					return;

				int adjuster = slidingExpirationParts.Length == 3 ? 0 : 1;
				int days = 0;
				int hours;
				int minutes;
				int seconds;
				if (!int.TryParse(slidingExpirationParts[0 + adjuster].Trim(), out hours))
					return;
				if (!int.TryParse(slidingExpirationParts[1 + adjuster].Trim(), out minutes))
					return;
				if (!int.TryParse(slidingExpirationParts[2 + adjuster].Trim(), out seconds))
					return;
				if (slidingExpirationParts.Length == 4)
					if (!int.TryParse(slidingExpirationParts[0].Trim(), out days))
						return;
				SlidingExpirationValue = slidingExpirationValue;
				if (slidingExpirationParts.Length == 4)
					SlidingExpiration = new TimeSpan(days, hours, minutes, seconds);
				else
					SlidingExpiration = new TimeSpan(hours, minutes, seconds);
			}
		}

		/// <summary>
		/// Start a <see cref="Task"/> that will call <see cref="RefreshSlidingExpirationValue"/> in a loop with a 1 second wait time between loops.
		/// </summary>
		protected virtual void StartRefreshSlidingExpirationValue()
		{
			Action action = () =>
			{
				long loop = 0;
				while (true)
				{
					RefreshSlidingExpirationValue();

					if (loop++ % 5 == 0)
						Thread.Yield();
					else
						Thread.Sleep(1000);
					if (loop == long.MaxValue)
						loop = long.MinValue;
				}
			};
			Task.Factory.StartNewSafely(action);
		}

		/// <summary>
		/// Get's a <see cref="CacheItemPolicy"/> with the <see cref="CacheItemPolicy.SlidingExpiration"/> set to 15 minutes
		/// </summary>
		protected virtual CacheItemPolicy GetDetaultCacheItemPolicy()
		{
			return new CacheItemPolicy { SlidingExpiration = SlidingExpiration };
		}
	}
}