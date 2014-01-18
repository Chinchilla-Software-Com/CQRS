using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEventHandler<TPermissionScope, in TEvent> : IHandler<TEvent>
		where TEvent : IEvent<TPermissionScope>
	{
	}
}