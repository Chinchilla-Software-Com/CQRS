namespace Chat.MicroServices.Conversations.Events
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Events;

	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that informs the system a new <see cref="Conversation"/> was started.
	/// </summary>
	[Serializable]
	[DataContract]
	[NotifyEveryoneEventAttribute]
	public class ConversationStarted : IEvent<Guid>
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

		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public Guid CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="T:Cqrs.Messages.IMessage"/> has been delivered to/sent via already.
		/// </summary>
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		/// <summary>
		/// The id of the conversation that was started.
		/// </summary>
		[DataMember]
		public Guid Rsn { get; set; }

		/// <summary>
		/// The name of conversation that was started.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		public ConversationStarted(Guid rsn, string name)
		{
			Rsn = rsn;
			Name = name;
		}
	}
}