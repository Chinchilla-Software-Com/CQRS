#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
#if NET40_OR_GREATER
using System.Data.Linq.Mapping;
#endif
using System.Runtime.Serialization;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// Captures all the data relevant to an <see cref="IEvent{TAuthenticationToken}"/> for an <see cref="IEventStore{TAuthenticationToken}"/> to persist.
	/// </summary>
	[Serializable]
	[DataContract]
#if NET40_OR_GREATER
	[Table(Name = "EventStore")]
#endif
	public class EventData
	{
		/// <summary>
		/// The data/content of the <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false, DbType = "text NOT NULL")]
#endif
		public object Data { get; set; }

		/// <summary>
		/// The identifier of the <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(IsPrimaryKey = true)]
#endif
		public Guid EventId { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/>
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false)]
#endif
		public string EventType { get; set; }

		/// <summary>
		/// The globally identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> , meaning it also includes <see cref="Type"/> information.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false)]
#endif
		public string AggregateId { get; set; }

		/// <summary>
		/// The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false)]
#endif
		public Guid AggregateRsn { get; set; }

		/// <summary>
		/// The new version number the targeted <see cref="IAggregateRoot{TAuthenticationToken}"/> or <see cref="ISaga{TAuthenticationToken}"/> that raised this.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false)]
#endif
		public long Version { get; set; }


		/// <summary>
		/// The date and time the event was raised or published.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false)]
#endif
		public DateTime Timestamp { get; set; }

		/// <summary>
		/// An identifier used to group together several <see cref="IMessage"/>. Any <see cref="IMessage"/> with the same <see cref="CorrelationId"/> were triggered by the same initiating request.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column(CanBeNull = false)]
#endif
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="EventData"/>.
		/// </summary>
		public EventData()
		{
			Timestamp = DateTime.UtcNow;
		}
	}
}