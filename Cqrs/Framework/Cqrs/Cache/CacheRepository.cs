using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Cache
{
	public class CacheRepository<TPermissionScope> : IRepository<TPermissionScope>
	{
		private IRepository<TPermissionScope> Repository { get; set; }

		private IEventStore<TPermissionScope> EventStore { get; set; }

		private MemoryCache Cache { get; set; }

		private Func<CacheItemPolicy> PolicyFactory { get; set; }

		private static readonly ConcurrentDictionary<string, object> Locks = new ConcurrentDictionary<string, object>();

		public CacheRepository(IRepository<TPermissionScope> repository, IEventStore<TPermissionScope> eventStore)
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

		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>
		{
			var idstring = aggregate.Id.ToString();
			try
			{
				lock (Locks.GetOrAdd(idstring, _ => new object()))
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

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TPermissionScope>> events = null)
			where TAggregateRoot : IAggregateRoot<TPermissionScope>
		{
			string idstring = aggregateId.ToString();
			try
			{
				IList<IEvent<TPermissionScope>> theseEvents = null;
				lock (Locks.GetOrAdd(idstring, _ => new object()))
				{
					TAggregateRoot aggregate;
					if (IsTracked(aggregateId))
					{
						aggregate = (TAggregateRoot)Cache.Get(idstring);
						theseEvents = events ?? EventStore.Get(aggregateId, aggregate.Version).ToList();
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