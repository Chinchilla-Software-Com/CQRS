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
	/// An <see cref="IEvent{TAuthenticationToken}"/> used specifically by a <see cref="ISaga{TAuthenticationToken}"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class SagaEvent<TAuthenticationToken>
		: ISagaEvent<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="SagaEvent{TAuthenticationToken}"/>.
		/// </summary>
		public SagaEvent() { }

		/// <summary>
		/// Instantiates a new instance of <see cref="SagaEvent{TAuthenticationToken}"/> with the provided <paramref name="event"/>.
		/// </summary>
		public SagaEvent(IEvent<TAuthenticationToken> @event)
		{
			Event = @event;
		}

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

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		/// <summary>
		/// The <typeparamref name="TAuthenticationToken"/> of the entity that triggered the event to be raised.
		/// </summary>
		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<TAuthenticationToken,TEvent>

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

		#region Implementation of ISagaEvent<TAuthenticationToken,TEvent>

		/// <summary>
		/// The <see cref="IEvent{TAuthenticationToken}"/> this <see cref="ISagaEvent{TAuthenticationToken}"/> encases.
		/// </summary>
		[DataMember]
		public IEvent<TAuthenticationToken> Event { get; set; }

		#endregion
	}
}