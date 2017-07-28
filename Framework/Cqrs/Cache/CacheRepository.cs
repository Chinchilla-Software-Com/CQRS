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
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Cache
{
	/// <summary>
	/// Uses <see cref="MemoryCache.Default"/> to provide a caching mechanism to improve performance of a <see cref="IAggregateRepository{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class CacheRepository<TAuthenticationToken> : IAggregateRepository<TAuthenticationToken>
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

		private static readonly ConcurrentDictionary<string, object> Locks = new ConcurrentDictionary<string, object>();

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
						object o;
						Locks.TryRemove(x.CacheItem.Key, out o);
					}
				};
		}

		/// <summary>
		/// Locks the cache, adds the provided <paramref name="aggregate"/> to the cache if not already in it, then calls <see cref="IAggregateRepository{TAuthenticationToken}.Save{TAggregateRoot}"/> on <see cref="Repository"/>.
		/// In the event of an <see cref="Exception"/> the <paramref name="aggregate"/> is always ejected out of the <see cref="Cache"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregate">The <see cref="IAggregateRoot{TAuthenticationToken}"/> to save and persist.</param>
		/// <param name="expectedVersion">The version number the <see cref="IAggregateRoot{TAuthenticationToken}"/> is expected to be at.</param>
		public virtual void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var idstring = aggregate.Id.ToString();
			try
			{
				lock (Locks.GetOrAdd(idstring, x => new object()))
				{
					if (aggregate.Id != Guid.Empty && !IsTracked(aggregate.Id))
						Cache.Add(idstring, aggregate, PolicyFactory.Invoke());
					Repository.Save(aggregate, expectedVersion);
				}
			}
			catch (Exception)
			{
				Cache.Remove(idstring);
				throw;
			}
		}

		/// <summary>
		/// Locks the cache, checks if the aggregate is tracked in the <see cref="Cache"/>, if it is
		/// retrieves the aggregate from the <see cref="Cache"/> and then uses either the provided <paramref name="events"/> or makes a call <see cref="IEventStore{TAuthenticationToken}.Get(System.Type,System.Guid,bool,int)"/> on the <see cref="EventStore"/>
		/// and rehydrates the cached aggregate with any new events from it's cached version.
		/// If the aggregate is not in the <see cref="Cache"/>
		/// <see cref="IAggregateRepository{TAuthenticationToken}.Get{TAggregateRoot}"/> is called on the <see cref="Repository"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public virtual TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			string idstring = aggregateId.ToString();
			try
			{
				IList<IEvent<TAuthenticationToken>> theseEvents = null;
				lock (Locks.GetOrAdd(idstring, _ => new object()))
				{
					TAggregateRoot aggregate;
					if (IsTracked(aggregateId))
					{
						aggregate = (TAggregateRoot)Cache.Get(idstring);
						theseEvents = events ?? EventStore.Get<TAggregateRoot>(aggregateId, false, aggregate.Version).ToList();
						if (theseEvents.Any() && theseEvents.First().Version != aggregate.Version + 1)
						{
							Cache.Remove(idstring);
						}
						else
						{
							aggregate.LoadFromHistory(theseEvents);
							return aggregate;
						}
					}

					aggregate = Repository.Get<TAggregateRoot>(aggregateId, theseEvents);
					Cache.Add(aggregateId.ToString(), aggregate, PolicyFactory.Invoke());
					return aggregate;
				}
			}
			catch (Exception)
			{
				Cache.Remove(idstring);
				throw;
			}
		}

		private bool IsTracked(Guid id)
		{
			return Cache.Contains(id.ToString());
		}
	}
}