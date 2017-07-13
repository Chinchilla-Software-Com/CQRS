using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Commands;

namespace Chat.MicroServices.Conversations.Commands
{
	[Serializable]
	[DataContract]
	public class PostCommentCommand : ICommand<Guid>
	{
		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="T:Cqrs.Messages.IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<Guid>

		[DataMember]
		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of ICommand<Guid>

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int ExpectedVersion { get; set; }

		#endregion
	}
}