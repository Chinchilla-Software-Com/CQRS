namespace Cqrs.Events
{
	public interface IEventDeserialiser<TAuthenticationToken>
	{
		IEvent<TAuthenticationToken> Deserialise(EventData eventData);
	}
}