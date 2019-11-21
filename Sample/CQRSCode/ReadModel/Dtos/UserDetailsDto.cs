using System;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;
using Cqrs.Entities;

namespace CQRSCode.ReadModel.Dtos
{
	[Table(Name = "UserDetailsDto")]
	public class UserDetailsDto : Entity
	{
		[DataMember]
		[Column(IsPrimaryKey = true)]
		public override Guid Rsn { get; set; }

		[DataMember]
		[Column]
		public override int SortingOrder { get; set; }

		[DataMember]
		[Column]
		public override bool IsDeleted { get; set; }

		public Guid Id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}

		[Column]
		public string Name;

		[Column]
		public int Version;

		public UserDetailsDto(Guid id, string name, int version)
		{
			Id = id;
			Name = name;
			Version = version;
		}

		public UserDetailsDto()
		{
		}
	}
}