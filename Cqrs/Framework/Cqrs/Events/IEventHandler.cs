using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEventHandler<in TEvent> : IHandler<TEvent>
		where TEvent : IEvent
	{
	}
}