using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommandHandler<in TCommand> : IHandler<TCommand>
		where TCommand : ICommand
	{
	}
}