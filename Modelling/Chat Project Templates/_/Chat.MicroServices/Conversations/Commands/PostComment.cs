namespace $safeprojectname$.Conversations.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Commands;

	/// <summary>
	/// A <see cref="ICommand{TAuthenticationToken}"/> that instructs the <see cref="Conversation"/> to post a comment into itself.
	/// </summary>
	[Serializable]
	[DataContract]
	public class PostComment : ICommand<Guid>
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
		/// The conversation the comment is to be posted into.
		/// </summary>
		[DataMember]
		public Guid ConversationRsn { get; set; }

		/// <summary>
		/// The content of the comment being posted.
		/// </summary>
		[DataMember]
		public string Comment { get; set; }

		/// <summary>
		/// The user who is posting the comment.
		/// </summary>
		[DataMember]
		public Guid UserRsn { get; set; }

		/// <summary>
		/// The name of the user who is posting the comment.
		/// </summary>
		[DataMember]
		public string UserName { get; set; }
	}
}