using System;

namespace Cqrs.Messages
{
	public interface IMessage
	{
		Guid CorrolationId { get; set; }
	}
}