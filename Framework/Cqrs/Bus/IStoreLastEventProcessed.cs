namespace Cqrs.Bus
{
	public interface IStoreLastEventProcessed
	{
		string EventLocation { get; set; }
	}
}