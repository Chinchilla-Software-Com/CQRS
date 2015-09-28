namespace Cqrs.Messages
{
	public interface IMessageHandler<in TMessage>
		: IHandler
		where TMessage: IMessage
	{
		void Handle(TMessage message);
	}
}