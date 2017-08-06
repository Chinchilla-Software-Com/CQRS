namespace $safeprojectname$.Conversations.Events
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Events;

	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that informs the system the <see cref="Conversation"/> was deleted.
	/// </summary>
	[Serializable]
	[DataContract]
	[NotifyEveryoneEventAttribute]
	public class ConversationDeleted : IEvent<Guid>
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
		/// The id of the conversation that was deleted.
		/// </summary>
		[DataMember]
		public Guid Rsn { get; set; }

		public ConversationDeleted(Guid rsn)
		{
			Rsn = rsn;
		}
	}
}