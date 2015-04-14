using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public  partial class InventoryItemRenamedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public InventoryItemRenamedEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(InventoryItemRenamed @event)
		{
			throw new NotImplementedException();
		}
	}
}
