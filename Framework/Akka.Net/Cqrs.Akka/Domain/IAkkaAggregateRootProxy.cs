using Akka.Actor;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaAggregateRootProxy<TAggregate>
	{
		IActorRef ActorReference { get; }

		TAggregate Aggregate { get; }
	}
}