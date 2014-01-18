namespace Cqrs.Commands
{
	public interface ICommandSender<TPermissionToken>
	{
		void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TPermissionToken>;
	}
}