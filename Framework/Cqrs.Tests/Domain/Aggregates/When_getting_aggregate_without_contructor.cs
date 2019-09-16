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
	public class When_getting_aggregate_without_contructor
	{
		private IUnitOfWork<ISingleSignOnToken> _unitOfWork;

		[SetUp]
		public void Setup()
		{
			var eventStore = new TestEventStore();
			var dependencyResolver = new TestDependencyResolver(eventStore);
			var aggregateFactory = new AggregateFactory(dependencyResolver, dependencyResolver.Resolve<ILogger>());
			var eventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(new AggregateRepository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher, new NullCorrelationIdHelper(), new ConfigurationManager()));
		}

		/*
		 * I don't think this makes sense anymore as it tests the test dependency resolver... test code testing test code.
		[Test]
		public void Should_throw_missing_parameterless_constructor_exception()
		{
			Assert.Throws<MissingParameterLessConstructorException>(() => _unitOfWork.Get<TestAggregateNoParameterLessConstructor>(Guid.NewGuid()));
		}
		*/
	}
}