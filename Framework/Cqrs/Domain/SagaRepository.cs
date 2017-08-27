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
using cdmdotnet.Logging;
using Cqrs.Domain.Exceptions;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	/// <summary>
	/// Provides basic repository methods for operations with instances of <see cref="ISaga{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class SagaRepository<TAuthenticationToken> : ISagaRepository<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the <see cref="IEventStore{TAuthenticationToken}"/> used to store and retrieve events from.
		/// </summary>
		protected IEventStore<TAuthenticationToken> EventStore { get; private set; }

		/// <summary>
		/// Gets or sets the Publisher used to publish events on once saved into the <see cref="EventStore"/>.
		/// </summary>
		protected IEventPublisher<TAuthenticationToken> Publisher { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="IAggregateFactory"/>.
		/// </summary>
		protected IAggregateFactory SagaFactory { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="SagaRepository{TAuthenticationToken}"/>
		/// </summary>
		public SagaRepository(IAggregateFactory sagaFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper)
		{
			EventStore = eventStore;
			Publisher = publisher;
			CorrelationIdHelper = correlationIdHelper;
			SagaFactory = sagaFactory;
		}

		/// <summary>
		/// Save and persist the provided <paramref name="saga"/>, optionally providing the version number the <see cref="ISaga{TAuthenticationToken}"/> is expected to be at.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of the <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="saga">The <see cref="ISaga{TAuthenticationToken}"/> to save and persist.</param>
		/// <param name="expectedVersion">The version number the <see cref="ISaga{TAuthenticationToken}"/> is expected to be at.</param>
		public virtual void Save<TSaga>(TSaga saga, int? expectedVersion = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			IList<ISagaEvent<TAuthenticationToken>> uncommittedChanges = saga.GetUncommittedChanges().ToList();
			if (!uncommittedChanges.Any())
				return;

			if (expectedVersion != null)
			{
				IEnumerable<IEvent<TAuthenticationToken>> eventStoreResults = EventStore.Get(saga.GetType(), saga.Id, false, expectedVersion.Value);
				if (eventStoreResults.Any())
					throw new ConcurrencyException(saga.Id);
			}

			var eventsToPublish = new List<ISagaEvent<TAuthenticationToken>>();

			int i = 0;
			int version = saga.Version;
			foreach (ISagaEvent<TAuthenticationToken> @event in uncommittedChanges)
			{
				if (@event.Id == Guid.Empty)
					@event.Id = saga.Id;
				if (@event.Id == Guid.Empty)
					throw new AggregateOrEventMissingIdException(saga.GetType(), @event.GetType());

				i++;
				version++;

				@event.Version = version;
				@event.TimeStamp = DateTimeOffset.UtcNow;
				@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();
				EventStore.Save(saga.GetType(), @event);
				eventsToPublish.Add(@event);
			}

			saga.MarkChangesAsCommitted();
			foreach (ISagaEvent<TAuthenticationToken> @event in eventsToPublish)
				PublishEvent(@event);
		}

		/// <summary>
		/// Publish the saved <paramref name="event"/>.
		/// </summary>
		protected virtual void PublishEvent(ISagaEvent<TAuthenticationToken> @event)
		{
			Publisher.Publish(@event);
		}

		/// <summary>
		/// Retrieves an <see cref="ISaga{TAuthenticationToken}"/> of type <typeparamref name="TSaga"/>.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of the <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="sagaId">The identifier of the <see cref="ISaga{TAuthenticationToken}"/> to retrieve.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="ISaga{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public virtual TSaga Get<TSaga>(Guid sagaId, IList<ISagaEvent<TAuthenticationToken>> events = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			return LoadSaga<TSaga>(sagaId, events);
		}

		/// <summary>
		/// Calls <see cref="IAggregateFactory.Create"/> to get a, <typeparamref name="TSaga"/>.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="id">The id of the <typeparamref name="TSaga"/> to create.</param>
		protected virtual TSaga CreateSaga<TSaga>(Guid id)
			where TSaga : ISaga<TAuthenticationToken>
		{
			var saga = SagaFactory.Create<TSaga>(id);

			return saga;
		}

		/// <summary>
		/// Calls <see cref="IAggregateFactory.Create"/> to get a, <typeparamref name="TSaga"/> and then calls <see cref="LoadSagaHistory{TSaga}"/>.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="id">The id of the <typeparamref name="TSaga"/> to create.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="ISaga{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		protected virtual TSaga LoadSaga<TSaga>(Guid id, IList<ISagaEvent<TAuthenticationToken>> events = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			var saga = SagaFactory.Create<TSaga>(id);

			LoadSagaHistory(saga, events);
			return saga;
		}

		/// <summary>
		/// If <paramref name="events"/> is null, loads the events from <see cref="EventStore"/>, checks for duplicates and then
		/// rehydrates the <paramref name="saga"/> with the events.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="saga">The <typeparamref name="TSaga"/> to rehydrate.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="ISaga{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		/// <param name="throwExceptionOnNoEvents">If true will throw an instance of <see cref="SagaNotFoundException{TSaga,TAuthenticationToken}"/> if no aggregate events or provided or found in the <see cref="EventStore"/>.</param>
		public virtual void LoadSagaHistory<TSaga>(TSaga saga, IList<ISagaEvent<TAuthenticationToken>> events = null, bool throwExceptionOnNoEvents = true)
			where TSaga : ISaga<TAuthenticationToken>
		{
			IList<ISagaEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TSaga>(saga.Id).Cast<ISagaEvent<TAuthenticationToken>>().ToList();
			if (!theseEvents.Any())
			{
				if (throwExceptionOnNoEvents)
					throw new SagaNotFoundException<TSaga, TAuthenticationToken>(saga.Id);
				return;
			}

			var duplicatedEvents =
				theseEvents.GroupBy(x => x.Version)
					.Select(x => new { Version = x.Key, Total = x.Count() })
					.FirstOrDefault(x => x.Total > 1);
			if (duplicatedEvents != null)
				throw new DuplicateSagaEventException<TSaga, TAuthenticationToken>(saga.Id, duplicatedEvents.Version);

			saga.LoadFromHistory(theseEvents);
		}
	}
}