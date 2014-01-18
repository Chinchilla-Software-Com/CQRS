using Cqrs.Events;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public interface IEventBuilder<TPermissionToken>
	{
		EventData CreateFrameworkEvent(string eventDataBody);

		EventData CreateFrameworkEvent(IEvent<TPermissionToken> eventData);

		EventData CreateFrameworkEvent(string type, IEvent<TPermissionToken> eventData);

		EventData CreateFrameworkEvent(string type, string eventDataBody);

		EventData CreateClientConnectedEvent(string clientName);
	}
}