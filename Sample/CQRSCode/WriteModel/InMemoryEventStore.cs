using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;
using Cqrs.Domain;

namespace CQRSCode.WriteModel
{
	public class InMemoryEventStore
		: IEventStore<ISingleSignOnToken>
	{
		private readonly Dictionary<Guid, List<IEvent<ISingleSignOnToken>>> _inMemoryDb = new Dictionary<Guid, List<IEvent<ISingleSignOnToken>>>();

		public void Save(Type aggregateRootType, IEvent<ISingleSignOnToken> @event)
		{
			List<IEvent<ISingleSignOnToken>> list;
			_inMemoryDb.TryGetValue(@event.Id, out list);
			if (list == null)
			{
				list = new List<IEvent<ISingleSignOnToken>>();
				_inMemoryDb.Add(@event.Id, list);
			}
			list.Add(@event);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			List<IEvent<ISingleSignOnToken>> events;
			_inMemoryDb.TryGetValue(aggregateId, out events);
			return events != null ? events.Where(x => x.Version > fromVersion) : new List<IEvent<ISingleSignOnToken>>();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public IEnumerable<IEvent<ISingleSignOnToken>> GetToVersion(Type aggregateRootType, Guid aggregateId, int version)
		{
			List<IEvent<ISingleSignOnToken>> events;
			_inMemoryDb.TryGetValue(aggregateId, out events);
			return events != null ? events.Where(x => x.Version <= version) : new List<IEvent<ISingleSignOnToken>>();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public IEnumerable<IEvent<ISingleSignOnToken>> GetToVersion<T>(Guid aggregateId, int version)
		{
			return GetToVersion(typeof(T), aggregateId, version);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public IEnumerable<IEvent<ISingleSignOnToken>> GetToDate(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			List<IEvent<ISingleSignOnToken>> events;
			_inMemoryDb.TryGetValue(aggregateId, out events);
			return events != null ? events.Where(x => x.TimeStamp <= versionedDate) : new List<IEvent<ISingleSignOnToken>>();
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public IEnumerable<IEvent<ISingleSignOnToken>> GetToDate<T>(Guid aggregateId, DateTime versionedDate)
		{
			return GetToDate(typeof(T), aggregateId, versionedDate);
		}

		public IEnumerable<EventData> Get(Guid correlationId)
		{
			return Enumerable.Empty<EventData>();
		}

		public void Save<T>(IEvent<ISingleSignOnToken> @event)
		{
			Save(typeof(T), @event);
		}
	}
}