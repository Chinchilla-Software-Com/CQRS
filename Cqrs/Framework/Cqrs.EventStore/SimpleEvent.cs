using System;
using Cqrs.Events;

namespace Cqrs.EventStore
{
	public class SimpleEvent : IEvent
	{
		public string Message { get; set; }

		#region Implementation of IEvent

		public Guid Id { get; set; }
		public int Version { get; set; }
		public DateTimeOffset TimeStamp { get; set; }

		#endregion
	}
}