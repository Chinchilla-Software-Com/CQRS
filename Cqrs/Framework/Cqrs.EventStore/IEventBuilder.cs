using Cqrs.Events;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public interface IEventBuilder<TPermissionScope>
	{
		EventData CreateFrameworkEvent(string eventDataBody);

		EventData CreateFrameworkEvent(IEvent<TPermissionScope> eventData);

		EventData CreateFrameworkEvent(string type, IEvent<TPermissionScope> eventData);

		EventData CreateFrameworkEvent(string type, string eventDataBody);

		EventData CreateClientConnectedEvent(string clientName);
	}
}