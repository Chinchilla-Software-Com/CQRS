using System;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;
using Cqrs.Entities;

namespace CQRSCode.ReadModel.Dtos
{
	[Table(Name = "InventoryItemListDto")]
	public class InventoryItemListDto : Entity
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
			get { return Rsn; }
			set { Rsn = value; }
		}

		[Column]
		public string Name;

		public InventoryItemListDto(Guid id, string name)
		{
			Id = id;
			Name = name;
		}

		public InventoryItemListDto()
		{
		}
	}
}