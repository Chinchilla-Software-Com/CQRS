using System;
using Cqrs.Events;

namespace Cqrs.EventStore
{
	public class SimpleEvent<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		public string Message { get; set; }

		#region Implementation of IEvent

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public string CorrolationId { get; set; }

		#endregion
	}
}