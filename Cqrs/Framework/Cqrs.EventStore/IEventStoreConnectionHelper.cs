using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public interface IEventStoreConnectionHelper
	{
		EventStoreConnection GetEventStoreConnection();
	}
}