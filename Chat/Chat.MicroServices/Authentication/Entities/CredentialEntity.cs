namespace Chat.MicroServices.Authentication.Entities
{
	using System;
	using System.Data.Linq.Mapping;
	using System.Runtime.Serialization;
	using Cqrs.Entities;

	[Serializable]
	[DataContract]
	[Table(Name = "Credentials")]
	public class CredentialEntity : Entity
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
		public Guid UserRsn { get; set; }

		[DataMember]
		[Column]
		public string Hash { get; set; }
	}
}