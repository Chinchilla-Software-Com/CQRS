using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEventHandler<TAuthenticationToken, in TEvent> : IHandler<TEvent>
		where TEvent : IEvent<TAuthenticationToken>
	{
	}
}