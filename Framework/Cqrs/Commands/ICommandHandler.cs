using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommandHandler<TAuthenticationToken, in TCommand> : IHandler<TCommand>
		where TCommand : ICommand<TAuthenticationToken>
	{
	}
}