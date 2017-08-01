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
	/// <summary>
	/// Captures all the data relevant to an <see cref="IEvent{TAuthenticationToken}"/> for <see cref="MongoDbEventStore{TAuthenticationToken}"/> to persist.
	/// </summary>
	[Serializable]
	[DataContract]
	public class MongoDbEventData : EventData
	{
		/// <summary>
		/// An internal MongoDB identifier.
		/// </summary>
		[DataMember]
		// ReSharper disable InconsistentNaming
		public ObjectId _id { get; set; }
		// ReSharper restore InconsistentNaming

		/// <summary>
		/// Instantiates a new instance of <see cref="MongoDbEventData"/>.
		/// </summary>
		public MongoDbEventData() { }

		/// <summary>
		/// Instantiates a new instance of <see cref="MongoDbEventData"/>.
		/// </summary>
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