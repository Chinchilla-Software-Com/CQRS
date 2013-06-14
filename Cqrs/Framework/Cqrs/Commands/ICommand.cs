using Cqrs.Messages;

namespace Cqrs.Commands
{
    public interface ICommand : IMessage
    {
        int ExpectedVersion { get; set; }
    }
}