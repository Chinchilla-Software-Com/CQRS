using Cqrs.Events;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public interface IEventBuilder<TAuthenticationToken>
	{
		EventData CreateFrameworkEvent(string eventDataBody);

		EventData CreateFrameworkEvent(IEvent<TAuthenticationToken> eventData);

		EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData);

		EventData CreateFrameworkEvent(string type, string eventDataBody);

		EventData CreateClientConnectedEvent(string clientName);
	}
}