#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Cache
{
	/// <summary>
	/// Uses <see cref="MemoryCache.Default"/> to provide a caching mechanism to improve performance of a <see cref="IAggregateRepository{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class CacheRepository<TAuthenticationToken>
		: IAggregateRepository<TAuthenticationToken>
	{
		/// <summary>
		/// Sets or set the <see cref="IAggregateRepository{TAuthenticationToken}"/> that will be used, and cached over.
		/// </summary>
		private IAggregateRepository<TAuthenticationToken> Repository { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="IEventStore{TAuthenticationToken}"/> used to retrieve events from when a cache hit occurs.
		/// </summary>
		private IEventStore<TAuthenticationToken> EventStore { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="MemoryCache"/> used.
		/// </summary>
		private MemoryCache Cache { get; set; }

		private Func<CacheItemPolicy> PolicyFactory { get; set; }

		private static readonly ConcurrentDictionary<string,
#if NET40
		object
#else
		SemaphoreSlim
#endif
		> Locks = new ConcurrentDictionary<string,
#if NET40
		object
#else
		SemaphoreSlim
#endif
		>();

		/// <summary>
		/// Instantiates a new instance of <see cref="CacheRepository{TAuthenticationToken}"/>.
		/// </summary>
		public CacheRepository(IAggregateRepository<TAuthenticationToken> repository, IEventStore<TAuthenticationToken> eventStore)
		{
			if(repository == null)
				throw new ArgumentNullException("repository");
			if(eventStore == null)
				throw new ArgumentNullException("eventStore");

			Repository = repository;
			EventStore = eventStore;
			Cache = MemoryCache.Default;
			PolicyFactory = () => new CacheItemPolicy
				{
					SlidingExpiration = new TimeSpan(0,0,15,0),
					RemovedCallback = x =>
					{
#if NET40
						object
#else
						SemaphoreSlim
#endif
						o;
						Locks.TryRemove(x.CacheItem.Key, out o);
					}
				};
		}

		/// <summary>
		/// Locks the cache, adds the provided <paramref name="aggregate"/> to the cache if not already in it, then calls IAggregateRepository{TAuthenticationToken}.Save on <see cref="Repository"/>.
		/// In the event of an <see cref="Exception"/> the <paramref name="aggregate"/> is always ejected out of the <see cref="Cache"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregate">The <see cref="IAggregateRoot{TAuthenticationToken}"/> to save and persist.</param>
		/// <param name="expectedVersion">The version number the <see cref="IAggregateRoot{TAuthenticationToken}"/> is expected to be at.</param>
		public virtual
#if NET40
			void Save
#else
			async Task SaveAsync
#endif
				<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var idString = aggregate.Id.ToString();
			try
			{
#if NET40
				lock (Locks.GetOrAdd(idString, x => new object()))
#else
				var lockObject = Locks.GetOrAdd(idString, x => new SemaphoreSlim(1, 1));
				await lockObject.WaitAsync();
				try
#endif
				{
					if (aggregate.Id != Guid.Empty && !IsTracked(aggregate.Id))
					Cache.Add(idString, aggregate, PolicyFactory.Invoke());
#if NET40
					Repository.Save
#else
					await Repository.SaveAsync
#endif
						(aggregate, expectedVersion);
				}
#if NET40
#else
				finally
				{
					//When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
					//This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
					lockObject.Release();
				}
#endif
			}
			catch (Exception)
			{
				Cache.Remove(idString);
				throw;
			}
		}

		/// <summary>
		/// Locks the cache, checks if the aggregate is tracked in the <see cref="Cache"/>, if it is
		/// retrieves the aggregate from the <see cref="Cache"/> and then uses either the provided <paramref name="events"/> or makes a call IEventStore{TAuthenticationToken}.Get on the <see cref="EventStore"/>
		/// and rehydrates the cached aggregate with any new events from it's cached version.
		/// If the aggregate is not in the <see cref="Cache"/>
		/// IAggregateRepository{TAuthenticationToken}.Get is called on the <see cref="Repository"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public virtual
#if NET40
			TAggregateRoot Get
#else
			async Task<TAggregateRoot> GetAsync
#endif
				<TAggregateRoot>(Guid aggregateId, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			string idString = aggregateId.ToString();
			try
			{
				IList<IEvent<TAuthenticationToken>> theseEvents = null;
#if NET40
				lock (Locks.GetOrAdd(idString, x => new object()))
#else
				var lockObject = Locks.GetOrAdd(idString, x => new SemaphoreSlim(1, 1));
				await lockObject.WaitAsync();
				try
#endif
				{
					TAggregateRoot aggregate;
					if (IsTracked(aggregateId))
					{
						aggregate = (TAggregateRoot)Cache.Get(idString);
						theseEvents = events;
						if (theseEvents == null)
						{
#if NET40
							theseEvents = EventStore.Get<TAggregateRoot>(aggregateId, false, aggregate.Version).ToList();
#else
							theseEvents = (await EventStore.GetAsync<TAggregateRoot>(aggregateId, false, aggregate.Version)).ToList();
#endif
						}
						if (theseEvents.Any() && theseEvents.First().Version != aggregate.Version + 1)
						{
							Cache.Remove(idString);
						}
						else
						{
							aggregate.LoadFromHistory(theseEvents);
							return aggregate;
						}
					}

#if NET40
					aggregate = Repository.Get<TAggregateRoot>(aggregateId, theseEvents);
#else
					aggregate = await Repository.GetAsync<TAggregateRoot>(aggregateId, theseEvents);
#endif
					Cache.Add(aggregateId.ToString(), aggregate, PolicyFactory.Invoke());
					return aggregate;
				}
#if NET40
#else
				finally
				{
					//When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
					//This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
					lockObject.Release();
				}
#endif
			}
			catch (Exception)
			{
				Cache.Remove(idString);
				throw;
			}
		}

		/// <summary>
		/// Retrieves an <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <typeparamref name="TAggregateRoot"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to retrieve.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public
#if NET40
		TAggregateRoot GetToVersion
#else
		Task<TAggregateRoot> GetToVersionAsync
#endif
			<TAggregateRoot>(Guid aggregateId, int version, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			throw new InvalidOperationException("Verion replay is not appriopriate with caching.");
		}

		/// <summary>
		/// Retrieves an <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <typeparamref name="TAggregateRoot"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to retrieve.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public
#if NET40
		TAggregateRoot GetToDate
#else
		Task<TAggregateRoot> GetToDateAsync
#endif
			<TAggregateRoot>(Guid aggregateId, DateTime versionedDate, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			throw new InvalidOperationException("Verion replay is not appriopriate with caching.");
		}

		private bool IsTracked(Guid id)
		{
			return Cache.Contains(id.ToString());
		}
	}
}