namespace Cqrs.Commands
{
	public interface ICommandReceiver<TAuthenticationToken>
	{
		void ReceiveCommand(ICommand<TAuthenticationToken> command);
	}
}