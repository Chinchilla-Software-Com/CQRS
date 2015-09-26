using System;

namespace Cqrs.Messages
{
	public interface IMessage
	{
		[Obsolete("Use CorrelationId")]
		Guid CorrolationId { get; set; }

		Guid CorrelationId { get; set; }

		FrameworkType Framework { get; set; }
	}
}