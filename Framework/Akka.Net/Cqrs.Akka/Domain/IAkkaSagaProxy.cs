using Akka.Actor;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaSagaProxy<out TSaga>
	{
		IActorRef ActorReference { get; }

		TSaga Saga { get; }
	}
}