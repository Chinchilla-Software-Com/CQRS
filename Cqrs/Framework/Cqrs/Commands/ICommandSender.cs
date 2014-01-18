namespace Cqrs.Commands
{
	public interface ICommandSender<TAuthenticationToken>
	{
		void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}