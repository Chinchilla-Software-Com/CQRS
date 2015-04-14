using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public  partial class ItemsRemovedFromInventoryEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public ItemsRemovedFromInventoryEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(ItemsRemovedFromInventory @event)
		{
			throw new NotImplementedException();
		}
	}
}
