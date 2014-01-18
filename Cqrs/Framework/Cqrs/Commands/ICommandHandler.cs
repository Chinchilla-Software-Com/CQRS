using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommandHandler<TPermissionScope, in TCommand> : IHandler<TCommand>
		where TCommand : ICommand<TPermissionScope>
	{
	}
}