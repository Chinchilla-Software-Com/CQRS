namespace Cqrs.Events
{
	public interface IEventPublisher<TPermissionScope>
	{
		void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TPermissionScope>;
	}
}