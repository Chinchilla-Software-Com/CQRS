namespace Chat.MicroServices.Conversations.Entities
{
	using System;
	using System.Data.Linq.Mapping;
	using System.Runtime.Serialization;
	using Cqrs.Entities;

	/// <summary>
	/// A summary of a conversation
	/// </summary>
	[Serializable]
	[DataContract]
	[Table(Name = "ConversationSummary")]
	public class ConversationSummaryEntity : Entity
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
		public string Name { get; set; }

		private int _messageCount;

		[DataMember]
		[Column]
		public int MessageCount
		{
			get
			{
				return _messageCount;
			}
			set
			{
				_messageCount = value;
			}
		}

		[DataMember]
		[Column]
		public DateTime LastUpdatedDate { get; set; }
	}
}