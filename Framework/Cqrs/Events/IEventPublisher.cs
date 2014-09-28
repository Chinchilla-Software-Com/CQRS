namespace Cqrs.Events
{
	public interface IEventPublisher<TAuthenticationToken>
	{
		void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;
	}
}