namespace $safeprojectname$.Conversations.Events
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Events;

	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that informs the system the <see cref="Conversation"/> posted a comment into itself.
	/// </summary>
	[Serializable]
	[DataContract]
	[NotifyEveryoneEventAttribute]
	public class CommentPosted : IEvent<Guid>
	{
		#region Implementation of IEvent

		[DataMember]
		public Guid Id
		{
			get
			{
				return Rsn;
			}
			set
			{
				Rsn = value;
			}
		}

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<Guid>

		[DataMember]
		public Guid AuthenticationToken { get; set; }

		#endregion

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

		/// <summary>
		/// The id of the comment that was posted.
		/// </summary>
		[DataMember]
		public Guid Rsn { get; set; }

		/// <summary>
		/// The conversation the comment was posted into.
		/// </summary>
		[DataMember]
		public Guid ConversationRsn
		{
			get { return Rsn; }
			set { Rsn = value; }
		}

		/// <summary>
		/// The identifier of the comment was posted into.
		/// </summary>
		[DataMember]
		public Guid MessageRsn { get; set; }

		/// <summary>
		/// The name of conversation the comment was posted into.
		/// </summary>
		[DataMember]
		public string ConversationName { get; set; }

		/// <summary>
		/// The user who posted the comment.
		/// </summary>
		[DataMember]
		public Guid UserRsn { get; set; }

		/// <summary>
		/// The name of the user who posted the comment.
		/// </summary>
		[DataMember]
		public string UserName { get; set; }

		/// <summary>
		/// The content of the comment that was posted.
		/// </summary>
		[DataMember]
		public string Comment { get; set; }

		/// <summary>
		/// When the comment that was posted.
		/// </summary>
		[DataMember]
		public DateTime DatePosted { get; set; }

		/// <summary>
		/// The current count of messages in the conversation.
		/// </summary>
		[DataMember]
		public int CurrentMessageCount { get; set; }

		public CommentPosted(Guid rsn, Guid messageRsn, string conversationName, Guid userRsn, string userName, string comment, DateTime datePosted, int currentMessageCount)
		{
			Rsn = rsn;
			MessageRsn = messageRsn;
			ConversationName = conversationName;
			UserRsn = userRsn;
			UserName = userName;
			Comment = comment;
			DatePosted = datePosted;
			CurrentMessageCount = currentMessageCount;
		}
	}
}