using System;
using cdmdotnet.Logging;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Domain.Factories;
using Cqrs.Authentication;
using Cqrs.Configuration;
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
		private AggregateRepository<ISingleSignOnToken> _rep;

		[SetUp]
		public void Setup()
		{
			_eventStore = new TestInMemoryEventStore();
			_eventPublisher = new TestEventPublisher();
			var dependencyResolver = new TestDependencyResolver(null);
			var aggregateFactory = new AggregateFactory(dependencyResolver, dependencyResolver.Resolve<ILogger>());
			_rep = new AggregateRepository<ISingleSignOnToken>(aggregateFactory, _eventStore, _eventPublisher, new NullCorrelationIdHelper(), new ConfigurationManager());

			_aggregate = new TestAggregate(Guid.Empty);

		}

		[Test]
		public void Should_throw_aggregate_or_event_missing_id_exception_from_repository()
		{
			Assert.Throws<AggregateOrEventMissingIdException>(() => _rep.Save(_aggregate, 0));
		}
	}
}