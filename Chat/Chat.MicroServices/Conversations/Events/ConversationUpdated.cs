namespace Chat.MicroServices.Conversations.Events
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Events;

	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that informs the system the <see cref="Conversation"/> updated its name.
	/// </summary>
	[Serializable]
	[DataContract]
	[NotifyEveryoneEventAttribute]
	public class ConversationUpdated : IEvent<Guid>
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
		/// The id of the conversation that was updated.
		/// </summary>
		[DataMember]
		public Guid Rsn { get; set; }

		/// <summary>
		/// The new name of conversation.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		public ConversationUpdated(Guid rsn, string name)
		{
			Rsn = rsn;
			Name = name;
		}
	}
}