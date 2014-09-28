namespace Cqrs.Domain.Factories
{
	public interface IAggregateFactory
	{
		TAggregate CreateAggregate<TAggregate>();
	}
}