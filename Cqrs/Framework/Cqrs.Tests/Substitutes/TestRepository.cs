using System;
using System.Collections.Generic;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Tests.Substitutes
{
	public class TestRepository : IRepository
	{
		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot
		{
			Saved = aggregate;
			if (expectedVersion == 100)
			{
				throw new Exception();
			}
		}

		public IAggregateRoot Saved { get; private set; }

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent> events = null)
			where TAggregateRoot : IAggregateRoot
		{
			var obj = (TAggregateRoot) Activator.CreateInstance(typeof (TAggregateRoot), true);
			obj.LoadFromHistory(new[] {new TestAggregateDidSomething {Id = aggregateId, Version = 1}});
			return obj;
		}
	}
}