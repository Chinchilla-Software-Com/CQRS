namespace Chat.MicroServices.Conversations.Entities
{
	using System;
	using System.Data.Linq.Mapping;
	using System.Runtime.Serialization;
	using Cqrs.Entities;

	/// <summary>
	/// A conversation message
	/// </summary>
	[Serializable]
	[DataContract]
	[Table(Name = "Messages")]
	public class MessageEntity : Entity
	{
		[DataMember]
		[Column(IsPrimaryKey = true)]
		public override Guid Rsn { get; set; }

		[DataMember]
		[Column]
		public override int SortingOrder { get; set; }

		[DataMember]
		[Column]
		public override bool IsLogicallyDeleted { get; set; }

		[DataMember]
		[Column]
		public Guid ConversationRsn { get; set; }

		[DataMember]
		[Column]
		public string ConversationName { get; set; }

		[DataMember]
		[Column]
		public Guid UserRsn { get; set; }

		[DataMember]
		[Column]
		public string UserName { get; set; }

		[DataMember]
		[Column]
		public string Content { get; set; }

		[DataMember]
		[Column]
		public DateTime DatePosted { get; set; }
	}
}