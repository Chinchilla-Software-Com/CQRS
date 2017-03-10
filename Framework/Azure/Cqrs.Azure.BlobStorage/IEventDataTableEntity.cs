namespace Cqrs.Azure.BlobStorage
{
	public interface IEventDataTableEntity<TEventData>
	{
		TEventData EventData { get; set; }
	}
}