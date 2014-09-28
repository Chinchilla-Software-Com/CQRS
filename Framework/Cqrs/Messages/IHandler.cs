namespace Cqrs.Messages
{
	public interface IHandler<in TMessage>
		where TMessage: IMessage
	{
		void Handle(TMessage message);
	}
}