namespace Chat.MicroServices.Conversations.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Commands;

	/// <summary>
	/// A <see cref="ICommand{TAuthenticationToken}"/> that instructs the system to start a new <see cref="Conversation"/>.
	/// </summary>
	[Serializable]
	[DataContract]
	public class StartConversation : ICommand<Guid>
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

		/// <summary>
		/// The name of the conversation.
		/// </summary>
		[DataMember]
		public string Name { get; set; }
	}
}