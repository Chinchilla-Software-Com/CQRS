using System;
using Cqrs.Entities;

namespace CQRSCode.ReadModel.Dtos
{
	public class InventoryItemListDto : Entity
	{
		public Guid Id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}
		public string Name;

		public InventoryItemListDto(Guid id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}