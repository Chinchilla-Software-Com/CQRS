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
using System.Threading.Tasks;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEventStore{TAuthenticationToken}"/> that uses a local (non-static) <see cref="IDictionary{TKey,TValue}"/>.
	/// This does not manage memory in any way and will continue to grow. Mostly suitable for running tests or short lived processes.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class InProcessEventStore<TAuthenticationToken>
		: IEventStore<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the in-memory storage <see cref="IDictionary{TKey,TValue}"/>.
		/// </summary>
		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> InMemoryDb { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="InProcessEventStore{TAuthenticationToken}"/> class.
		/// </summary>
		public InProcessEventStore()
		{
			InMemoryDb = new Dictionary<Guid, IList<IEvent<TAuthenticationToken>>>();
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
			IList<IEvent<TAuthenticationToken>> list;
			InMemoryDb.TryGetValue(@event.GetIdentity(), out list);
			if (list == null)
			{
				list = new List<IEvent<TAuthenticationToken>>();
				InMemoryDb.Add(@event.GetIdentity(), list);
			}
			list.Add(@event);
#if NET40
#else
			await Task.CompletedTask;
#endif
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
			return
#if NET40
				Get
#else
				await GetAsync
#endif
					(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> Get
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetAsync
#endif
				(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			IList<IEvent<TAuthenticationToken>> events;
			InMemoryDb.TryGetValue(aggregateId, out events);
			var results = events != null
				? events.Where(x => x.Version > fromVersion)
				: new List<IEvent<TAuthenticationToken>>();
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
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToVersion
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToVersionAsync
#endif
				(Type aggregateRootType, Guid aggregateId, int version)
		{
			IList<IEvent<TAuthenticationToken>> events;
			InMemoryDb.TryGetValue(aggregateId, out events);
			var results = events != null
				? events.Where(x => x.Version <= version)
				: new List<IEvent<TAuthenticationToken>>();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

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
			return
#if NET40
				GetToVersion
#else
				await GetToVersionAsync
#endif
					(typeof(T), aggregateId, version);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetToDate
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetToDateAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			IList<IEvent<TAuthenticationToken>> events;
			InMemoryDb.TryGetValue(aggregateId, out events);
			var results= events != null
				? events.Where(x => x.TimeStamp <= versionedDate)
				: new List<IEvent<TAuthenticationToken>>();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

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
			return
#if NET40
				GetToDate
#else
				await GetToDateAsync
#endif
					(typeof(T), aggregateId, versionedDate);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public virtual
#if NET40
			IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates
#else
			async Task<IEnumerable<IEvent<TAuthenticationToken>>> GetBetweenDatesAsync
#endif
				(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate,
			DateTime toVersionedDate)
		{
			IList<IEvent<TAuthenticationToken>> events;
			InMemoryDb.TryGetValue(aggregateId, out events);
			var results = events != null
				? events.Where(eventData => eventData.TimeStamp >= fromVersionedDate && eventData.TimeStamp <= toVersionedDate)
				: new List<IEvent<TAuthenticationToken>>();
			return
#if NET40
				results;
#else
				await Task.FromResult(results);
#endif
		}

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
			return
#if NET40
				GetBetweenDates
#else
				await GetBetweenDatesAsync
#endif
					(typeof(T), aggregateId, fromVersionedDate, toVersionedDate);
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public virtual
#if NET40
			IEnumerable<EventData> Get
#else
			async Task<IEnumerable<EventData>> GetAsync
#endif
				(Guid correlationId)
		{
			var result = Enumerable.Empty<EventData>();
			return
#if NET40
				result;
#else
				await Task.FromResult(result);
#endif
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
	}
}