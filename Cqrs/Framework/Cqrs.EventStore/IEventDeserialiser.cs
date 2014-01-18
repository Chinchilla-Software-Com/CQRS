using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	public interface IEventDeserialiser<TPermissionToken>
	{
		IEvent<TPermissionToken> Deserialise(RecordedEvent eventData);

		IEvent<TPermissionToken> Deserialise(ResolvedEvent notification);

		JsonSerializerSettings GetSerialisationSettings();
	}
}