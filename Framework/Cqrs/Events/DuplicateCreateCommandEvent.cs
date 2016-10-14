#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Messages;

namespace Cqrs.Events
{
	[Serializable]
	[DataContract]
	public class DuplicateCreateCommandEvent<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[DataMember]
		[Obsolete("Use Frameworks, It's far more flexible and OriginatingFramework")]
		public FrameworkType Framework { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		public IEnumerable<string> Frameworks { get; set; }

		[Obsolete("Use CorrelationId")]
		[DataMember]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<TAuthenticationToken>

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		[DataMember]
		public Type AggregateType { get; set; }

		[DataMember]
		public Guid AggregateRsn { get; set; }
	}
}