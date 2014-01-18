using System;
using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEvent<TAuthenticationToken> : IMessageWithAuthenticationToken<TAuthenticationToken>
	{
		Guid Id { get; set; }

		int Version { get; set; }

		DateTimeOffset TimeStamp { get; set; }
	}
}

