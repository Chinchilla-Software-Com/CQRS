using System;
using Cqrs.Cache;
using Cqrs.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Cache
{
	[TestFixture]
	public class When_saving_aggregate
	{
		private CacheRepository<ISingleSignOnToken> _rep;
		private TestAggregate _aggregate;
		private TestRepository _testRep;

		[SetUp]
		public void Setup()
		{
			_testRep = new TestRepository();
			_rep = new CacheRepository<ISingleSignOnToken>(_testRep, new TestInMemoryEventStore());
			_aggregate = _testRep.Get<TestAggregate>(Guid.NewGuid());
			_aggregate.DoSomething();
			_rep.Save(_aggregate,-1);
		}

		[Test]
		public void Should_get_same_aggregate_on_get()
		{
			var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
			Assert.That(aggregate, Is.EqualTo(_aggregate));
		}

		[Test]
		public void Should_save_to_repository()
		{
			Assert.That(_testRep.Saved, Is.EqualTo(_aggregate));
		}

		[Test]
		public void Should_not_cache_empty_id()
		{
			var aggregate = new TestAggregate(Guid.Empty);
			_rep.Save(aggregate);
			Assert.That(_rep.Get<TestAggregate>(Guid.Empty), Is.Not.EqualTo(aggregate));
		}
	}
}