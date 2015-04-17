using System;
using Cqrs.Entities;

namespace CQRSCode.ReadModel.Dtos
{
	public class InventoryItemDetailsDto : Entity
	{
		public Guid Id
		{
			get {return Rsn;}
			set { Rsn = value; }
		}
		public string Name;
		public int CurrentCount;
		public int Version;

		public InventoryItemDetailsDto(Guid id, string name, int currentCount, int version)
		{
			Id = id;
			Name = name;
			CurrentCount = currentCount;
			Version = version;
		}
	}
}