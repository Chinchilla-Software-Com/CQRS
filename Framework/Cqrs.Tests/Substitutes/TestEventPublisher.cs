using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestEventPublisher : IEventPublisher<ISingleSignOnToken>
	{
		public void Publish<T>(T @event)
			where T : IEvent<ISingleSignOnToken>
		{
			Published++;
		}

		public void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<ISingleSignOnToken>
		{
			Published = Published + events.Count();
		}

		public int Published { get; private set; }
	}
}