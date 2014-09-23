namespace Cqrs.Events
{
	public interface IEventBuilder<TAuthenticationToken>
	{
		EventData CreateFrameworkEvent(IEvent<TAuthenticationToken> eventData);

		EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData);
	}
}