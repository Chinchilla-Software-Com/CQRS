using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	public interface IEventDeserialiser<TPermissionScope>
	{
		IEvent<TPermissionScope> Deserialise(RecordedEvent eventData);

		IEvent<TPermissionScope> Deserialise(ResolvedEvent notification);

		JsonSerializerSettings GetSerialisationSettings();
	}
}