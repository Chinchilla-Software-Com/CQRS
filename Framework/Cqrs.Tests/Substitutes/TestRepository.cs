using System;
using System.Collections.Generic;
using System.Reflection;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestRepository : IRepository<ISingleSignOnToken>
	{
		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<ISingleSignOnToken>
		{
			Saved = aggregate;
			if (expectedVersion == 100)
			{
				throw new Exception();
			}
		}

		public IAggregateRoot<ISingleSignOnToken> Saved { get; private set; }

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<ISingleSignOnToken>> events = null)
			where TAggregateRoot : IAggregateRoot<ISingleSignOnToken>
		{
			var obj = (TAggregateRoot)Activator.CreateInstance(typeof(TAggregateRoot), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { aggregateId }, null);
			var testAggregate = obj as TestAggregate;
			if (testAggregate != null)
				testAggregate.SetId(aggregateId);
			obj.LoadFromHistory(new[] { new TestAggregateDidSomething { Id = aggregateId, Version = 2 } });
			return obj;
		}
	}
}