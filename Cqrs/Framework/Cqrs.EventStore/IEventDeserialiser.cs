using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	public interface IEventDeserialiser
	{
		IEvent Deserialise(RecordedEvent eventData);
		IEvent Deserialise(ResolvedEvent notification);

		JsonSerializerSettings GetSerialisationSettings();
	}
}