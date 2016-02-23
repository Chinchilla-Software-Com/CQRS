using System;
using cdmdotnet.AutoMapper;
using Cqrs.Repositories.Queries;
using MyCompany.MyProject.Domain.Inventory.Entities;
using MyCompany.MyProject.Domain.Inventory.Repositories;
using MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies;

namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public  partial class InventoryItemRenamedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IInventoryItemRepository InventoryItemRepository { get; private set; }

		protected IInventoryItemSummaryRepository InventoryItemSummaryRepository { get; private set; }

		public InventoryItemRenamedEventHandler(IAutomapHelper automapHelper, IQueryFactory queryFactory, IInventoryItemRepository inventoryItemRepository, IInventoryItemSummaryRepository inventoryItemSummaryRepository)
		{
			AutomapHelper = automapHelper;
			QueryFactory = queryFactory;
			InventoryItemRepository = inventoryItemRepository;
			InventoryItemSummaryRepository = inventoryItemSummaryRepository;
		}

		partial void OnHandle(InventoryItemRenamed @event)
		{
			UpdateDetailedItem(@event);
			UpdateSummaryItem(@event);
		}

		private void UpdateDetailedItem(InventoryItemRenamed @event)
		{
			// Define Query
			ISingleResultQuery<InventoryItemQueryStrategy, InventoryItemEntity> query = QueryFactory.CreateNewSingleResultQuery<InventoryItemQueryStrategy, InventoryItemEntity>();
			query.QueryStrategy.WithRsn(@event.Rsn);

			// Retrieve Data, but remember if no items exist, the value is null
			query = InventoryItemRepository.Retrieve(query, false);
			InventoryItemEntity inventoryItem = query.Result;

			// As a previous event will have created this instance we should throw an exception if it is not found.
			if (inventoryItem == null)
				throw new NullReferenceException(string.Format("No entity was found by the id '{0}'", @event.Rsn));

			inventoryItem.Name = @event.NewName;

			// Again, this MUST be an update as commented above.
			InventoryItemRepository.Update(inventoryItem);
		}

		private void UpdateSummaryItem(InventoryItemRenamed @event)
		{
			// Define Query
			ISingleResultQuery<InventoryItemSummaryQueryStrategy, InventoryItemSummaryEntity> query = QueryFactory.CreateNewSingleResultQuery<InventoryItemSummaryQueryStrategy, InventoryItemSummaryEntity>();
			query.QueryStrategy.WithRsn(@event.Rsn);

			// Retrieve Data, but remember if no items exist, the value is null
			query = InventoryItemSummaryRepository.Retrieve(query, false);
			InventoryItemSummaryEntity inventoryItem = query.Result;

			// As a previous event will have created this instance we should throw an exception if it is not found.
			if (inventoryItem == null)
				throw new NullReferenceException(string.Format("No entity was found by the id '{0}'", @event.Rsn));

			inventoryItem.Name = @event.NewName;

			// Again, this MUST be an update as commented above.
			InventoryItemSummaryRepository.Update(inventoryItem);
		}
	}
}
