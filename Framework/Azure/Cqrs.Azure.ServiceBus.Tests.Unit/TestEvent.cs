using System;
using Cqrs.Events;

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	public class TestEvent
		: IEvent<Guid>
	{
		#region Implementation of IMessageWithAuthenticationToken<Guid>

		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<Guid>

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#endregion
	}
}