namespace Cqrs.Events
{
	public interface IEventPublisher<TPermissionToken>
	{
		void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TPermissionToken>;
	}
}