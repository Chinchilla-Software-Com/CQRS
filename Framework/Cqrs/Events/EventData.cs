#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Data.Linq.Mapping;

namespace Cqrs.Events
{
	[Table(Name = "EventStore")]
	public class EventData
	{
		[Column(CanBeNull = false, DbType = "text NOT NULL")]
		public object Data { get; set; }

		[Column(IsPrimaryKey = true)]
		public Guid EventId { get; set; }

		[Column(CanBeNull = false)]
		public string EventType { get; set; }

		[Column(CanBeNull = false)]
		public string AggregateId { get; set; }

		[Column(CanBeNull = false)]
		public long Version { get; set; }

		[Column(CanBeNull = false)]
		public DateTime Timestamp { get; set; }

		[Column(CanBeNull = false)]
		public Guid CorrelationId { get; set; }

		public EventData()
		{
			Timestamp = DateTime.UtcNow;
		}
	}
}