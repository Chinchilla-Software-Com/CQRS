using System;
using Cqrs.Entities;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;

namespace CQRSCode.ReadModel.Dtos
{
	[Table(Name = "InventoryItemDetailsDto")]
	public class InventoryItemDetailsDto : Entity
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

		public Guid Id
		{
			get {return Rsn;}
			set { Rsn = value; }
		}

		[Column]
		public string Name;

		[Column]
		public int CurrentCount;

		[Column]
		public int Version;

		public InventoryItemDetailsDto(Guid id, string name, int currentCount, int version)
		{
			Id = id;
			Name = name;
			CurrentCount = currentCount;
			Version = version;
		}

		public InventoryItemDetailsDto()
		{
		}
	}
}