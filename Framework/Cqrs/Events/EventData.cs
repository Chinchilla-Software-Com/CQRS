#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Events
{
	public class EventData
	{
		public object Data { get; set; }

		public Guid EventId { get; set; }

		public string EventType { get; set; }

		public string AggregateId { get; set; }

		public long Version { get; set; }

		public DateTime Timestamp { get; set; }

		public EventData()
		{
			Timestamp = DateTime.UtcNow;
		}
	}
}