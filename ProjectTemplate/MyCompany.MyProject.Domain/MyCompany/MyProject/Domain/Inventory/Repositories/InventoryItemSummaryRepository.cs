using Cqrs.Repositories;
using MyCompany.MyProject.Domain.Factories;
using MyCompany.MyProject.Domain.Inventory.Entities;
using MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies;

namespace MyCompany.MyProject.Domain.Inventory.Repositories
{
	public class InventoryItemSummaryRepository : Repository<InventoryItemSummaryQueryStrategy, InventoryItemSummaryQueryStrategyBuilder, InventoryItemSummaryEntity>, IInventoryItemSummaryRepository
	{
		public InventoryItemSummaryRepository(IDomainDataStoreFactory dataStoreFactory, InventoryItemSummaryQueryStrategyBuilder inventoryItemSummaryQueryBuilder)
			: base(dataStoreFactory.GetInventoryItemSummaryDataStore, inventoryItemSummaryQueryBuilder)
		{
		}
	}
}
