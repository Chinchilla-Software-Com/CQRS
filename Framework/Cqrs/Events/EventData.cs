#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;

namespace Cqrs.Events
{
	[Serializable]
	[DataContract]
	[Table(Name = "EventStore")]
	public class EventData
	{
		[DataMember]
		[Column(CanBeNull = false, DbType = "text NOT NULL")]
		public object Data { get; set; }

		[DataMember]
		[Column(IsPrimaryKey = true)]
		public Guid EventId { get; set; }

		[DataMember]
		[Column(CanBeNull = false)]
		public string EventType { get; set; }

		[DataMember]
		[Column(CanBeNull = false)]
		public string AggregateId { get; set; }

		[DataMember]
		[Column(CanBeNull = false)]
		public Guid AggregateRsn { get; set; }

		[DataMember]
		[Column(CanBeNull = false)]
		public long Version { get; set; }

		[DataMember]
		[Column(CanBeNull = false)]
		public DateTime Timestamp { get; set; }

		[DataMember]
		[Column(CanBeNull = false)]
		public Guid CorrelationId { get; set; }

		public EventData()
		{
			Timestamp = DateTime.UtcNow;
		}
	}
}