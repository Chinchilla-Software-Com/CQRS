using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommandHandler<TAuthenticationToken, in TCommand>
		: IMessageHandler<TCommand>
		, ICommandHandle
		where TCommand : ICommand<TAuthenticationToken>
	{
	}

	public interface ICommandHandle : IHandler
	{
	}
}