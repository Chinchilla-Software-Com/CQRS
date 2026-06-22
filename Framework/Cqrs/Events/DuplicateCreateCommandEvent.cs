#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that informs the system that an operation resulted in a duplicate.
	/// </summary>
	[Serializable]
	[DataContract]
	public class DuplicateCreateCommandEvent<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		#region Implementation of IMessage

		/// <summary>
		/// The identifier of the command itself.
		/// In some cases this may be the <see cref="IAggregateRoot{TAuthenticationToken}"/> or <see cref="ISaga{TAuthenticationToken}"/> this command targets.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The new version number the targeted <see cref="IAggregateRoot{TAuthenticationToken}"/> or <see cref="ISaga{TAuthenticationToken}"/> that raised this.
		/// </summary>
		[DataMember]
		public int Version { get; set; }

		/// <summary>
		/// The date and time the event was raised or published.
		/// </summary>
		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }


		#endregion

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		/// <summary>
		/// The <typeparamref name="TAuthenticationToken"/> of the entity that triggered the event to be raised.
		/// </summary>
		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		/// <summary>
		/// An identifier used to group together several <see cref="IMessage"/>. Any <see cref="IMessage"/> with the same <see cref="CorrelationId"/> were triggered by the same initiating request.
		/// </summary>
		[DataMember]
		public Guid CorrelationId { get; set; }

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

		#endregion

		/// <summary>
		/// The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> that already exists, but had another <see cref="DtoAggregateEventType.Created"/> event raised.
		/// </summary>
		[DataMember]
		public Type AggregateType { get; set; }

		/// <summary>
		/// The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that already exists, but had another <see cref="DtoAggregateEventType.Created"/> event raised.
		/// </summary>
		[DataMember]
		public Guid AggregateRsn { get; set; }
	}
}