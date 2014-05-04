namespace Cqrs.EventStore.Bus
{
	public interface IStoreLastEventProcessed
	{
		string EventLocation { get; set; }
	}
}