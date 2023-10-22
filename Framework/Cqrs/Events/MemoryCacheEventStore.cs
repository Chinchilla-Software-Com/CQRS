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
			EventStoreByType = new MemoryCache($"EventStoreByType-{id:N}");
			EventStoreByCorrelationId = new MemoryCache($"EventStoreByCorrelationId-{id:N}");

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
		public override
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> Get
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync
#endif
				(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			if (!EventStoreByType.Contains(streamName))
			{
				Logger.LogDebug($"The event store has no items '{streamName}'.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			CacheItem item = EventStoreByType.GetCacheItem(streamName);
			if (item == null)
			{
				Logger.LogDebug($"The event store had an item '{streamName}' but doesn't now.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			var events = item.Value as IEnumerable<EventData>;
			if (events == null)
			{
				if (item.Value == null)
					Logger.LogDebug($"The event store had an item '{streamName}' but it was null.");
				else
					Logger.LogWarning($"The event store had an item '{streamName}' but it was of type {item.Value.GetType()}.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}
			IEnumerable<EventData> query = events
				.Where(eventData => eventData.Version > fromVersion)
				.OrderByDescending(eventData => eventData.Version);

			if (useLastEventOnly)
				query = query.AsQueryable().Take(1);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public override
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToVersion
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToVersionAsync
#endif
				(Type aggregateRootType, Guid aggregateId, int version)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			if (!EventStoreByType.Contains(streamName))
			{
				Logger.LogDebug($"The event store has no items '{streamName}'.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			CacheItem item = EventStoreByType.GetCacheItem(streamName);
			if (item == null)
			{
				Logger.LogDebug($"The event store had an item '{streamName}' but doesn't now.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			var events = item.Value as IEnumerable<EventData>;
			if (events == null)
			{
				if (item.Value == null)
					Logger.LogDebug($"The event store had an item '{streamName}' but it was null.");
				else
					Logger.LogWarning($"The event store had an item '{streamName}' but it was of type {item.Value.GetType()}.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}
			IEnumerable<EventData> query = events
				.Where(eventData => eventData.Version <= version)
				.OrderByDescending(eventData => eventData.Version);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToDate
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToDateAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			if (!EventStoreByType.Contains(streamName))
			{
				Logger.LogDebug($"The event store has no items '{streamName}'.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			CacheItem item = EventStoreByType.GetCacheItem(streamName);
			if (item == null)
			{
				Logger.LogDebug($"The event store had an item '{streamName}' but doesn't now.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}

			var events = item.Value as IEnumerable<EventData>;
			if (events == null)
			{
				if (item.Value == null)
					Logger.LogDebug($"The event store had an item '{streamName}' but it was null.");
				else
					Logger.LogWarning($"The event store had an item '{streamName}' but it was of type {item.Value.GetType()}.");
				return Enumerable.Empty<IEvent<TAuthenticationToken>>();
			}
			IEnumerable<EventData> query = events
				.Where(eventData => eventData.Timestamp <= versionedDate)
				.OrderByDescending(eventData => eventData.Version);

			var results = query
				.Select(EventDeserialiser.Deserialise)
				.ToList();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates
#else
			Task<IEnumerable<IEvent<TAuthenticationToken>>> GetBetweenDatesAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public override
#if NET40
			IEnumerable<EventData> Get
#else
			async Task<IEnumerable<EventData>> GetAsync
#endif
				(Guid correlationId)
		{
			if (!EventStoreByCorrelationId.Contains(correlationId.ToString("N")))
			{
				Logger.LogDebug($"The event store has no items by the correlationId '{correlationId:N}'.");
				return Enumerable.Empty<EventData>();
			}

			CacheItem item = EventStoreByCorrelationId.GetCacheItem(correlationId.ToString("N"));
			if (item == null)
			{
				Logger.LogDebug($"The event store had some items by the correlationId '{correlationId:N}' but doesn't now.");
				return Enumerable.Empty<EventData>();
			}

			var events = item.Value as IEnumerable<EventData>;
			if (events == null)
			{
				if (item.Value == null)
					Logger.LogDebug($"The event store had some items by the correlationId '{correlationId:N}' but it was null.");
				else
					Logger.LogWarning($"The event store had some items by the correlationId '{correlationId:N}' but it was of type {item.Value.GetType()}.");
				return Enumerable.Empty<EventData>();
			}
			IEnumerable<EventData> query = events.OrderBy(eventData => eventData.Timestamp);

			var results = query.ToList();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override
#if NET40
			void PersistEvent
#else
			async Task PersistEventAsync
#endif
				(EventData eventData)
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
					Logger.LogWarning($"The event store had some items by the correlationId '{correlationId:N}' but it doesn't now.");
					throw new Exception($"The event store had some items by the correlationId '{correlationId:N}' but it doesn't now.");
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
					Logger.LogWarning($"The event store had an item by id '{streamName}' but it doesn't now.");
					throw new Exception($"The event store had an item by id '{streamName}' but it doesn't now.");
				}
			}

			events.Add(eventData);
#if NET40
#else
			await Task.CompletedTask;
#endif
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