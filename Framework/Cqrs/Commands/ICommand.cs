using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommand<TAuthenticationToken> : IMessageWithAuthenticationToken<TAuthenticationToken>
	{
		int ExpectedVersion { get; set; }
	}
}