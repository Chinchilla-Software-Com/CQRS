using System;

namespace Cqrs.Events
{
	public class EventData
	{
		public byte[] Data { get; set; }

		public Guid EventId { get; set; }

		public string EventType { get; set; }

		public string AggregateId { get; set; }

		public DateTime Timestamp { get; set; }

		public EventData()
		{
			Timestamp = DateTime.UtcNow;
		}
	}
}