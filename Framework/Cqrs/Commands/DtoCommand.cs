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

namespace Cqrs.Commands
{
	/// <summary>
	/// A <see cref="ICommand{TPermissionToken}"/> for <see cref="IDto"/> objects
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TDto">The <see cref="Type"/> of <see cref="IDto"/> this command targets.</typeparam>
	[Serializable]
	[DataContract]
	public class DtoCommand<TAuthenticationToken, TDto> : ICommand<TAuthenticationToken>
		where TDto : IDto
	{
		/// <summary>
		/// Gets or sets the original version of the <typeparamref name="TDto"/>.
		/// </summary>
		[DataMember]
		public TDto Original { get; set; }

		/// <summary>
		/// Gets or sets the new version of the <typeparamref name="TDto"/>.
		/// </summary>
		[DataMember]
		public TDto New { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="DtoCommand{TAuthenticationToken,TDto}"/>
		/// </summary>
		public DtoCommand(Guid id, TDto original, TDto @new)
		{
			Id = id;
			Original = original;
			New = @new;
		}

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