using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Cache
{
	public class CacheRepository : IRepository
	{
		private IRepository Repository { get; set; }
		private IEventStore EventStore { get; set; }
		private MemoryCache Cache { get; set; }
		private Func<CacheItemPolicy> PolicyFactory { get; set; }
		private static readonly ConcurrentDictionary<string, object> Locks = new ConcurrentDictionary<string, object>();

		public CacheRepository(IRepository repository, IEventStore eventStore)
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
			where TAggregateRoot : IAggregateRoot
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

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId)
			where TAggregateRoot : IAggregateRoot
		{
			string idstring = aggregateId.ToString();
			try
			{
				lock (Locks.GetOrAdd(idstring, _ => new object()))
				{
					TAggregateRoot aggregate;
					if (IsTracked(aggregateId))
					{
						aggregate = (TAggregateRoot)Cache.Get(idstring);
						IList<IEvent> events = EventStore.Get(aggregateId, aggregate.Version).ToList();
						if (events.Any() && events.First().Version != aggregate.Version + 1)
						{
							Cache.Remove(idstring);
						}
						else
						{
							aggregate.LoadFromHistory(events);
							return aggregate;
						}
					}

					aggregate = Repository.Get<TAggregateRoot>(aggregateId);
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