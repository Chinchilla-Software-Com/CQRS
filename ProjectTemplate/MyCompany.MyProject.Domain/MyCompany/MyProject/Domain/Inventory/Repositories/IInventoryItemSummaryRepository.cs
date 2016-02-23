using Cqrs.Repositories;
using MyCompany.MyProject.Domain.Inventory.Entities;
using MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies;

namespace MyCompany.MyProject.Domain.Inventory.Repositories
{
	public interface IInventoryItemSummaryRepository : IRepository<InventoryItemSummaryQueryStrategy, InventoryItemSummaryEntity>
	{
	}
}

namespace MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies
{
	public partial class InventoryItemSummaryQueryStrategy
	{
	}
}
