namespace Cqrs.Messages
{
	public interface IMessage
	{
		string CorrolationId { get; set; }
	}
}