namespace Cqrs.Commands
{
	public interface ICommandSender<TPermissionScope>
	{
		void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TPermissionScope>;
	}
}