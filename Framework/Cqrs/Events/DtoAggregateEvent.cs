#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Events
{
	public class DtoAggregateEvent<TAuthenticationToken, TDto> : IEvent<TAuthenticationToken>
		where TDto : IDto
	{
		[DataMember]
		public TDto Original { get; private set; }

		[DataMember]
		public TDto New { get; private set; }

		public DtoAggregateEvent(Guid id, TDto original, TDto @new)
		{
			Id = id;
			Original = original;
			New = @new;
		}

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		public DtoAggregateEventType GetEventType()
		{
			if (New != null && Original == null)
				return DtoAggregateEventType.Created;
			if (New != null && Original != null)
				return DtoAggregateEventType.Updated;
			if (New == null && Original != null)
				return DtoAggregateEventType.Deleted;
			return DtoAggregateEventType.Unknown;
		}

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[DataMember]
		public FrameworkType Framework { get; set; }

		[Obsolete("Use CorrelationId")]
		[DataMember]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion
	}
}