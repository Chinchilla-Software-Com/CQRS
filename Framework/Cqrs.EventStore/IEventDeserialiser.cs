using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	public interface IEventDeserialiser<TAuthenticationToken>
	{
		IEvent<TAuthenticationToken> Deserialise(RecordedEvent eventData);

		IEvent<TAuthenticationToken> Deserialise(ResolvedEvent notification);

		JsonSerializerSettings GetSerialisationSettings();
	}
}