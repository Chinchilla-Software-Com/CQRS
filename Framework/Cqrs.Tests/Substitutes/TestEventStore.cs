using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestEventStore : IEventStore<ISingleSignOnToken>
	{
		public TestEventStore()
		{
			EmptyGuid = Guid.NewGuid();
			SavedEvents = new List<IEvent<ISingleSignOnToken>>();
		}

		public Guid EmptyGuid { get; private set; }

		public virtual void Save(Type aggregateRootType, IEvent<ISingleSignOnToken> @event)
		{
			SavedEvents.Add(@event);
		}

		public virtual IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T),aggregateId, useLastEventOnly, fromVersion);
		}

		public virtual IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			if (aggregateId == EmptyGuid || aggregateId == Guid.Empty)
			{
				return new List<IEvent<ISingleSignOnToken>>();
			}

			if (aggregateType == typeof (TestSaga))
			{
				return new List<ISagaEvent<ISingleSignOnToken>>
				{
					new SagaEvent<ISingleSignOnToken>
					(
						new TestAggregateDidSomething {Id = Guid.NewGuid(), Version = 26}
					){Id = aggregateId, Version = 2},
					new SagaEvent<ISingleSignOnToken>
					(
						new TestAggregateDidSomethingElse {Id = Guid.NewGuid(), Version = 13}
					){Id = aggregateId, Version = 3},
					new SagaEvent<ISingleSignOnToken>
					(
						new TestAggregateDidSomething {Id = Guid.NewGuid(), Version = 47}
					){Id = aggregateId, Version = 4}
				}
				.Where(x => x.Version > fromVersion);
			}
			return new List<IEvent<ISingleSignOnToken>>
			{
				new TestAggregateDidSomething {Id = aggregateId, Version = 2},
				new TestAggregateDidSomethingElse {Id = aggregateId, Version = 3},
				new TestAggregateDidSomething {Id = aggregateId, Version = 4}
			}
			.Where(x => x.Version > fromVersion);
		}

		public IEnumerable<EventData> Get(Guid correlationId)
		{
			return Enumerable.Empty<EventData>();
		}

		public virtual void Save<T>(IEvent<ISingleSignOnToken> @event)
		{
			Save(typeof(T), @event);
		}

		private List<IEvent<ISingleSignOnToken>> SavedEvents { get; set; }
	}
}