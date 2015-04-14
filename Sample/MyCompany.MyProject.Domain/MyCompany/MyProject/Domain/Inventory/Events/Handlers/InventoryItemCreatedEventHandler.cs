using cdmdotnet.AutoMapper;
using Cqrs.Repositories.Queries;
using MyCompany.MyProject.Domain.Inventory.Entities;
using MyCompany.MyProject.Domain.Inventory.Repositories;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public partial class InventoryItemCreatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IInventoryItemRepository InventoryItemRepository { get; private set; }

		protected IInventoryItemSummaryRepository InventoryItemSummaryRepository { get; private set; }

		public InventoryItemCreatedEventHandler(IAutomapHelper automapHelper, IQueryFactory queryFactory, IInventoryItemRepository inventoryItemRepository, IInventoryItemSummaryRepository inventoryItemSummaryRepository)
		{
			AutomapHelper = automapHelper;
			QueryFactory = queryFactory;
			InventoryItemRepository = inventoryItemRepository;
			InventoryItemSummaryRepository = inventoryItemSummaryRepository;
		}

		partial void OnHandle(InventoryItemCreated @event)
		{
			UpdateDetailedItem(@event);
			UpdateSummaryItem(@event);
		}

		private void UpdateDetailedItem(InventoryItemCreated @event)
		{
			InventoryItemEntity inventoryItem = new InventoryItemEntity
			{
				Rsn = @event.Rsn,
				Name = @event.Name,
				CurrentCount = 0,
				IsLogicallyDeleted = false
			};

			// As this is the first event for this instance, no existing instance will exist, thus a create operation is executed
			InventoryItemRepository.Create(inventoryItem);
		}

		private void UpdateSummaryItem(InventoryItemCreated @event)
		{
			InventoryItemSummaryEntity inventoryItem = new InventoryItemSummaryEntity { Rsn = @event.Rsn, Name = @event.Name };

			// As this is the first event for this instance, no existing instance will exist, thus a create operation is executed
			InventoryItemSummaryRepository.Create(inventoryItem);
		}
	}
}