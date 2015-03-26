using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Authentication;
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
			var aggregateFactory = new AggregateFactory(null);

			var snapshotStrategy = new DefaultSnapshotStrategy<ISingleSignOnToken>();
			var repository = new SnapshotRepository<ISingleSignOnToken>(snapshotstorage, snapshotStrategy, new Repository<ISingleSignOnToken>(aggregateFactory, eventstorage, eventpublisher), eventstorage, aggregateFactory);
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

		public Snapshot Get(Guid id)
		{
			return Snapshot;
		}

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

		public void Publish<T>(T @event) where T : IEvent<ISingleSignOnToken>
		{
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

		public void Save(IEvent<ISingleSignOnToken> @event, Type aggregateRootType)
		{
			Events.Add(@event);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Events.Where(x => x.Version > fromVersion);
		}
	}
}
