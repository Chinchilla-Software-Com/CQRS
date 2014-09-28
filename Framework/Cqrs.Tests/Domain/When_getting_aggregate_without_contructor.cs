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
	public class When_getting_aggregate_without_contructor
	{
		private IUnitOfWork<ISingleSignOnToken> _unitOfWork;

		[SetUp]
		public void Setup()
		{
			var aggregateFactory = new AggregateFactory();
			var eventStore = new TestInMemoryEventStore();
			var eventPublisher = new TestEventPublisher();
			_unitOfWork = new UnitOfWork<ISingleSignOnToken>(new Repository<ISingleSignOnToken>(aggregateFactory, eventStore, eventPublisher));
		}

		[Test]
		public void Should_throw_missing_parameterless_constructor_exception()
		{
			Assert.Throws<MissingParameterLessConstructorException>(() => _unitOfWork.Get<TestAggregateNoParameterLessConstructor>(Guid.NewGuid()));
		}
	}
}