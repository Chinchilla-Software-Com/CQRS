using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public  partial class InventoryItemCreatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public InventoryItemCreatedEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(InventoryItemCreated @event)
		{
			throw new NotImplementedException();
		}
	}
}
