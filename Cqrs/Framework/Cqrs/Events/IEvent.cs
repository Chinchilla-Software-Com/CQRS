using System;
using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEvent<TPermissionToken> : IMessageWithPermissionToken<TPermissionToken>
	{
		Guid Id { get; set; }

		int Version { get; set; }

		DateTimeOffset TimeStamp { get; set; }
	}
}

