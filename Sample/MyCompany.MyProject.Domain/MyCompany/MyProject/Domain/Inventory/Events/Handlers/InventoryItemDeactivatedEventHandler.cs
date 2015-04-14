using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public  partial class InventoryItemDeactivatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public InventoryItemDeactivatedEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(InventoryItemDeactivated @event)
		{
			throw new NotImplementedException();
		}
	}
}
