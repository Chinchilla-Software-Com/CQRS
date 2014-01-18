using Cqrs.Messages;

namespace Cqrs.Commands
{
	public interface ICommand<TPermissionScope> : IMessageWithPermissionScope<TPermissionScope>
	{
		int ExpectedVersion { get; set; }
	}
}