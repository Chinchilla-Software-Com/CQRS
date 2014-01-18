using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEventHandler<TPermissionToken, in TEvent> : IHandler<TEvent>
		where TEvent : IEvent<TPermissionToken>
	{
	}
}