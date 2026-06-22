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
using Cqrs.Entities;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	/// <summary>
	/// A Test <see cref="IEvent{TAuthenticationToken}"/>.
	/// </summary>
	[Serializable]
	[DataContract]
	public class TestEvent
		: Entity
		, IEvent<Guid>
	{
		#region Implementation of IMessageWithAuthenticationToken<Guid>

		/// <summary>
		/// The authentication token of the entity that triggered the event to be raised.
		/// </summary>
		[DataMember]
		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<Guid>

		/// <summary>
		/// The identifier of the event itself.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The new version number.
		/// </summary>
		[DataMember]
		public int Version { get; set; }

		/// <summary>
		/// The date and time the event was raised or published.
		/// </summary>
		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }


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
		/// A <see cref="decimal"/> that is sensative and should be encrypted in transit
		/// </summary>
		[DataMember]
		[Encrypted(KeyName = "TestKey")]
		public decimal SensitiveDecimal { get; set; }
	}
}