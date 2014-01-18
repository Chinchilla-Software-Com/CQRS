using System;
using System.Runtime.Caching;
using Cqrs.Cache;
using Cqrs.Authentication;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.Cache
{
	[TestFixture]
	public class When_saving_fails
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
			try
			{
				_rep.Save(_aggregate, 100);
			}
			catch (Exception){}
		}

		[Test]
		public void Should_evict_old_object_from_cache()
		{
			var aggregate = MemoryCache.Default.Get(_aggregate.Id.ToString());
			Assert.That(aggregate, Is.Null);
		}
	}
}