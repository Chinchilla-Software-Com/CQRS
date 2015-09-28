using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEventHandler<TAuthenticationToken, in TEvent>
		: IMessageHandler<TEvent>
		, IEventHandler
		where TEvent : IEvent<TAuthenticationToken>
	{
	}
	public interface IEventHandler
		: IHandler
	{
	}
}