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
using Chinchilla.Logging;
using Cqrs.Domain;
using Cqrs.Messages;

#if NET40
#else
using System.Threading.Tasks;
#endif

namespace Cqrs.Events
{
	/// <summary>
	/// Stores instances of <see cref="IEvent{TAuthenticationToken}"/> for replay, <see cref="IAggregateRoot{TAuthenticationToken}"/> and <see cref="ISaga{TAuthenticationToken}"/> rehydration.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class EventStore<TAuthenticationToken>
		: IEventStore<TAuthenticationToken>
	{
		/// <summary>
		/// The pattern used to generate the stream name.
		/// </summary>
		protected const string CqrsEventStoreStreamNamePattern = "{0}/{1}";

		/// <summary>
		/// The <see cref="IEventBuilder{TAuthenticationToken}"/> used to build events.
		/// </summary>
		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		/// <summary>
		/// The <see cref="IEventDeserialiser{TAuthenticationToken}"/> used to deserialise events.
		/// </summary>
		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		/// <summary>
		/// The <see cref="ITelemetryHelper"/> to use.
		/// </summary>
		protected ITelemetryHelper TelemetryHelper { get; set; }

		/// <summary>
		/// The <see cref="ILogger"/> to use.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="EventStore{TAuthenticationToken}"/>.
		/// </summary>
		protected EventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
			Logger = logger;
			TelemetryHelper = new NullTelemetryHelper();
		}

		/// <summary>
		/// Saves the provided <paramref name="event"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to be saved.</param>
		public virtual
#if NET40
			void Save
#else
			async Task SaveAsync
#endif
				<T>(IEvent<TAuthenticationToken> @event)
		{
#if NET40
			Save
#else
			await SaveAsync
#endif
				(typeof(T), @event);
		}

		/// <summary>
		/// Generate a unique stream name based on the provided <paramref name="aggregateRootType"/> and the <see cref="IEvent{TAuthenticationToken}.Id"/> from the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to extract information from.</param>
		protected virtual string GenerateStreamName(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			return GenerateStreamName(aggregateRootType, @event.GetIdentity());
		}

		/// <summary>
		/// Generate a unique stream name based on the provided <paramref name="aggregateRootType"/> and the <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		protected virtual string GenerateStreamName(Type aggregateRootType, Guid aggregateId)
		{
			return string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
		}

		/// <summary>
		/// Saves the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to be saved.</param>
		public virtual
#if NET40
			void Save
#else
			async Task SaveAsync
#endif
				(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			Logger.LogDebug($"Saving aggregate root event type '{@event.GetType().FullName}'", $"{GetType().Name}\\Save");
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			string streamName = GenerateStreamName(aggregateRootType, @event);
			eventData.AggregateId = streamName;
			eventData.AggregateRsn = @event.GetIdentity();
			eventData.Version = @event.Version;
			eventData.CorrelationId = @event.CorrelationId;
#if NET40
			PersistEvent
#else
			await PersistEventAsync
#endif
				(eventData);
			Logger.LogInfo($"Saving aggregate root event type '{@event.GetType().FullName}'... done", $"{GetType().Name}\\Save");
			TelemetryHelper.TrackMetric($"Cqrs/EventStore/Save/{streamName}", 1);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> Get
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync
#endif
				<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			IEnumerable<IEvent<TAuthenticationToken>> results = (
#if NET40
			Get
#else
			await GetAsync
#endif
				(typeof (T), aggregateId, useLastEventOnly, fromVersion)).ToList();
			TelemetryHelper.TrackMetric(string.Format("Cqrs/EventStore/Get/{0}", GenerateStreamName(typeof(T), aggregateId)), results.Count());

			return results;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		public abstract
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> Get
#else
			Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync
#endif
				(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public abstract
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToVersion
#else
			Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToVersionAsync
#endif
				(Type aggregateRootType, Guid aggregateId, int version);

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToVersion
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToVersionAsync
#endif
				<T>(Guid aggregateId, int version)
		{
			IEnumerable<IEvent<TAuthenticationToken>> results = (
#if NET40
			GetToVersion
#else
			await GetToVersionAsync
#endif
				(typeof(T), aggregateId, version)).ToList();
			TelemetryHelper.TrackMetric(string.Format("Cqrs/EventStore/GetToVersion/{0}", GenerateStreamName(typeof(T), aggregateId)), results.Count());

			return results;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public abstract
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToDate
#else
			Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToDateAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime versionedDate);

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToDate
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToDateAsync
#endif
				<T>(Guid aggregateId, DateTime versionedDate)
		{
			IEnumerable<IEvent<TAuthenticationToken>> results = (
#if NET40
			GetToDate
#else
			await GetToDateAsync
#endif
				(typeof(T), aggregateId, versionedDate)).ToList();
			TelemetryHelper.TrackMetric(string.Format("Cqrs/EventStore/GetToDate/{0}", GenerateStreamName(typeof(T), aggregateId)), results.Count());

			return results;
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public abstract
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates
#else
			Task<IEnumerable<IEvent<TAuthenticationToken>>> GetBetweenDatesAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate);

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetBetweenDatesAsync
#endif
				<T>(Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			IEnumerable<IEvent<TAuthenticationToken>> results = (
#if NET40
			GetBetweenDates
#else
			await GetBetweenDatesAsync
#endif
				(typeof(T), aggregateId, fromVersionedDate, toVersionedDate)).ToList();
			TelemetryHelper.TrackMetric(string.Format("Cqrs/EventStore/GetBetweenDates/{0}", GenerateStreamName(typeof(T), aggregateId)), results.Count());

			return results;
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public abstract
#if NET40
			IEnumerable<EventData> Get
#else
			Task<IEnumerable<EventData>> GetAsync
#endif
				(Guid correlationId);

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into storage.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected abstract
#if NET40
			void PersistEvent
#else
			Task PersistEventAsync
#endif
				(EventData eventData);
	}
}