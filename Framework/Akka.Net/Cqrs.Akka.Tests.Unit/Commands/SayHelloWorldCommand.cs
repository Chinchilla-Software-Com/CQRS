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
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Akka.Tests.Unit.Commands
{
	/// <summary>
	/// Someone wants to say "Hello World".
	/// </summary>
	public class SayHelloWorldCommand : ICommand<Guid>
	{
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

		#region Implementation of IMessageWithAuthenticationToken<Guid>

		/// <summary>
		/// The authentication token of the entity that triggered the event to be raised.
		/// </summary>
		[DataMember]
		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of ICommand<Guid>

		/// <summary>
		/// The identifier of the command itself.
		/// In some cases this may be the <see cref="IAggregateRoot{TAuthenticationToken}"/> or <see cref="ISaga{TAuthenticationToken}"/> this command targets.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The expected version number the targeted <see cref="IAggregateRoot{TAuthenticationToken}"/> or <see cref="ISaga{TAuthenticationToken}"/> is expected to be.
		/// </summary>
		[DataMember]
		public int ExpectedVersion { get; set; }

		#endregion
	}
}