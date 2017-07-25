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
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.EventStore
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that holds the event data in <see cref="Message"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class SimpleEvent<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		/// <summary>
		/// A serialised version of one or more instances of <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		public string Message { get; set; }

		#region Implementation of IEvent

		/// <summary>
		/// The ID of the <see cref="IEvent{TAuthenticationToken}"/>
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The version of the <see cref="IEvent{TAuthenticationToken}"/>
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
	}
}