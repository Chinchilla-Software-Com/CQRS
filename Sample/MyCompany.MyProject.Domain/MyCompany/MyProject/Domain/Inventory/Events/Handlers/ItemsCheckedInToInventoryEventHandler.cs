using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public  partial class ItemsCheckedInToInventoryEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public ItemsCheckedInToInventoryEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(ItemsCheckedInToInventory @event)
		{
			throw new NotImplementedException();
		}
	}
}
