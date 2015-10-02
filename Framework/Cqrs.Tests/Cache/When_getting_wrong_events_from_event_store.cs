using System;
using System.Runtime.Caching;
using Cqrs.Cache;
using Cqrs.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Cache
{
	[TestFixture]
	public class When_getting_earlier_than_expected_events_from_event_store
	{
		private CacheRepository<ISingleSignOnToken> _rep;
		private TestAggregate _aggregate;

		[SetUp]
		public void Setup()
		{
			_rep = new CacheRepository<ISingleSignOnToken>(new TestRepository(), new TestEventStoreWithBugs());
			_aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
		}

		[Test]
		public void Should_evict_old_object_from_cache()
		{
			_rep.Get<TestAggregate>(_aggregate.Id);
			var aggregate = MemoryCache.Default.Get(_aggregate.Id.ToString());
			Assert.That(aggregate, Is.Not.EqualTo(_aggregate));
		}

		[Test]
		public void Should_get_events_from_start()
		{
			var aggregate =_rep.Get<TestAggregate>(_aggregate.Id);
			Assert.That(aggregate.Version, Is.EqualTo(4));
		}
	}
}