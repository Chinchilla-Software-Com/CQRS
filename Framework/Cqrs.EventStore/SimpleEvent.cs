using System;
using System.Runtime.Serialization;
using Cqrs.Events;

namespace Cqrs.EventStore
{
	public class SimpleEvent<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		[DataMember]
		public string Message { get; set; }

		#region Implementation of IEvent

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public string CorrolationId { get; set; }

		#endregion
	}
}