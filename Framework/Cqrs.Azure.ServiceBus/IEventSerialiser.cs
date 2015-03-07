using Cqrs.Events;

namespace Cqrs.Azure.ServiceBus
{
	public interface IEventSerialiser<TAuthenticationToken>
	{
		string SerialisEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;

		IEvent<TAuthenticationToken> DeserialisEvent(string @event);
	}
}