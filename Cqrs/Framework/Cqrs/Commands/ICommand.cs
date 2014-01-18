using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommand<TPermissionToken> : IMessageWithPermissionToken<TPermissionToken>
	{
		int ExpectedVersion { get; set; }
	}
}