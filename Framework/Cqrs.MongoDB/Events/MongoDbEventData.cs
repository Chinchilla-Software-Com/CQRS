#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Events;
using MongoDB.Bson;

namespace Cqrs.MongoDB.Events
{
	[Serializable]
	[DataContract]
	public class MongoDbEventData : EventData
	{
		[DataMember]
		// ReSharper disable InconsistentNaming
		public ObjectId _id { get; set; }
		// ReSharper restore InconsistentNaming


		public MongoDbEventData() { }

		public MongoDbEventData(EventData eventData)
		{
			AggregateRsn = eventData.AggregateRsn;
			CorrelationId = eventData.CorrelationId;
			AggregateId = eventData.AggregateId;
			Data = eventData.Data;
			EventId = eventData.EventId;
			EventType = eventData.EventType;
			Timestamp = eventData.Timestamp;
			Version = eventData.Version;
		}
	}
}