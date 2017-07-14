namespace Chat.MicroServices.Authentication.Entities
{
	using System;
	using System.Data.Linq.Mapping;
	using System.Runtime.Serialization;
	using Cqrs.Entities;

	/// <summary>
	/// A User
	/// </summary>
	[Serializable]
	[DataContract]
	[Table(Name = "Users")]
	public class UserEntity : Entity
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
		public string FirstName { get; set; }

		[DataMember]
		[Column]
		public string LastName { get; set; }
	}
}