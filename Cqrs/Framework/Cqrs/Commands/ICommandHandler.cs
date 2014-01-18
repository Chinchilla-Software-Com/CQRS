using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommandHandler<TPermissionToken, in TCommand> : IHandler<TCommand>
		where TCommand : ICommand<TPermissionToken>
	{
	}
}