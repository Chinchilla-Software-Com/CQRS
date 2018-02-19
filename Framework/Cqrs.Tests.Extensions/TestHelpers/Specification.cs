using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Snapshots;
using NUnit.Framework;

namespace Cqrs.Tests.Extensions.TestHelpers
{
	[TestFixture]
	public abstract class Specification<TAggregate, THandler, TCommand>
		where TAggregate : AggregateRoot<ISingleSignOnToken>
		where THandler : class, ICommandHandler<ISingleSignOnToken, TCommand>
		where TCommand : ICommand<ISingleSignOnToken>
	{

		protected TAggregate Aggregate { get; set; }
		protected IUnitOfWork<ISingleSignOnToken> UnitOfWork { get; set; }
		protected abstract IEnumerable<IEvent<ISingleSignOnToken>> Given();
		protected abstract TCommand When();
		protected abstract THandler BuildHandler();

		protected Snapshot Snapshot { get; set; }
		protected IList<IEvent<ISingleSignOnToken>> EventDescriptors { get; set; }
		protected IList<IEvent<ISingleSignOnToken>> PublishedEvents { get; set; }
		
		[SetUp]
		public void Run()
		{
			var eventstorage = new SpecEventStorage(Given().ToList());
			var snapshotstorage = new SpecSnapShotStorage(Snapshot);
			var eventpublisher = new SpecEventPublisher();
			var aggregateFactory = new AggregateFactory(null, new ConsoleLogger(new LoggerSettings(), new CorrelationIdHelper(new ThreadedContextItemCollectionFactory())));

			var snapshotStrategy = new DefaultSnapshotStrategy<ISingleSignOnToken>();
			var repository = new SnapshotRepository<ISingleSignOnToken>(snapshotstorage, snapshotStrategy, new AggregateRepository<ISingleSignOnToken>(aggregateFactory, eventstorage, eventpublisher, new NullCorrelationIdHelper(), new ConfigurationManager()), eventstorage, aggregateFactory);
			UnitOfWork = new UnitOfWork<ISingleSignOnToken>(repository);

			Aggregate = UnitOfWork.Get<TAggregate>(Guid.Empty);

			var handler = BuildHandler();
			handler.Handle(When());

			Snapshot = snapshotstorage.Snapshot;
			PublishedEvents = eventpublisher.PublishedEvents;
			EventDescriptors = eventstorage.Events;
		}
	}

	internal class SpecSnapShotStorage : ISnapshotStore
	{
		public SpecSnapShotStorage(Snapshot snapshot)
		{
			Snapshot = snapshot;
		}

		public Snapshot Snapshot { get; set; }

		/// <summary>
		/// Get the latest <see cref="Snapshots.Snapshot"/> from storage.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to find a snapshot for.</typeparam>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get the most recent <see cref="Snapshots.Snapshot"/> of.</param>
		/// <returns>The most recent <see cref="Snapshots.Snapshot"/> of</returns>
		public Snapshot Get<TAggregateRoot>(Guid id)
		{
			return Snapshot;
		}

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshots.Snapshot"/> to save and store.</param>
		public void Save(Snapshot snapshot)
		{
			Snapshot = snapshot;
		}
	}

	internal class SpecEventPublisher : IEventPublisher<ISingleSignOnToken>
	{
		public SpecEventPublisher()
		{
			PublishedEvents = new List<IEvent<ISingleSignOnToken>>();
		}

		public void Publish<T>(T @event)
			where T : IEvent<ISingleSignOnToken>
		{
			PublishedEvents.Add(@event);
		}

		public void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<ISingleSignOnToken>
		{
			foreach (TEvent @event in events)
				PublishedEvents.Add(@event);
		}

		public IList<IEvent<ISingleSignOnToken>> PublishedEvents { get; set; }
	}

	internal class SpecEventStorage : IEventStore<ISingleSignOnToken>
	{
		public SpecEventStorage(IList<IEvent<ISingleSignOnToken>> events)
		{
			Events = events;
		}

		public IList<IEvent<ISingleSignOnToken>> Events { get; set; }

		public void Save<T>(IEvent<ISingleSignOnToken> @event)
		{
			Events.Add(@event);
		}

		public void Save(Type aggregateRootType, IEvent<ISingleSignOnToken> @event)
		{
			Events.Add(@event);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Events.Where(x => x.Version > fromVersion);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public IEnumerable<IEvent<ISingleSignOnToken>> GetToVersion(Type aggregateRootType, Guid aggregateId, int version)
		{
			return Events.Where(x => x.Version <= version);
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
			return Events.Where(x => x.TimeStamp <= versionedDate);
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
	}
}