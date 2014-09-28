using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestEventPublisher : IEventPublisher<ISingleSignOnToken>
	{
		public void Publish<T>(T @event) where T : IEvent<ISingleSignOnToken>
		{
			Published++;
		}

		public int Published { get; private set; }
	}
}