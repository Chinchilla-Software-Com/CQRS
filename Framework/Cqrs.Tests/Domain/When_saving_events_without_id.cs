using System;
using Cqrs.Domain;
using Cqrs.Domain.Exception;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Domain
{
	[TestFixture]
	public class When_saving_events_without_id
	{
		private TestInMemoryEventStore _eventStore;
		private TestAggregate _aggregate;
		private TestEventPublisher _eventPublisher;
		private Repository<ISingleSignOnToken> _rep;

		[SetUp]
		public void Setup()
		{
			_eventStore = new TestInMemoryEventStore();
			_eventPublisher = new TestEventPublisher();
			var aggregateFactory = new AggregateFactory(null);
			_rep = new Repository<ISingleSignOnToken>(aggregateFactory, _eventStore, _eventPublisher);

			_aggregate = new TestAggregate(Guid.Empty);

		}

		[Test]
		public void Should_throw_aggregate_or_event_missing_id_exception_from_repository()
		{
			Assert.Throws<AggregateOrEventMissingIdException>(() => _rep.Save(_aggregate, 0));
		}
	}
}