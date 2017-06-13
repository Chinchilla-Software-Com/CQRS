using Akka.Actor;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaAggregateRootProxy<out TAggregate>
	{
		IActorRef ActorReference { get; }

		TAggregate Aggregate { get; }
	}
}