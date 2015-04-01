using System;
using System.Runtime.Serialization;
using Cqrs.Events;

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	public class TestEvent : IEvent<Guid>
	{
		#region Implementation of IMessageWithAuthenticationToken<Guid>

		[DataMember]
		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<Guid>

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrolationId { get; set; }

		#endregion
	}
}