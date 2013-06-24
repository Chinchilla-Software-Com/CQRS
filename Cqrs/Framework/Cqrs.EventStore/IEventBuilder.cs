using Cqrs.Events;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public interface IEventBuilder
	{
		EventData CreateFrameworkEvent(string eventDataBody);
		EventData CreateFrameworkEvent(IEvent eventData);
		EventData CreateFrameworkEvent(string type, IEvent eventData);
		EventData CreateFrameworkEvent(string type, string eventDataBody);
		EventData CreateClientConnectedEvent(string clientName);
	}
}