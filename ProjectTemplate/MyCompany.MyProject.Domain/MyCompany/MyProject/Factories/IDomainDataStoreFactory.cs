using Cqrs.DataStores;

namespace MyCompany.MyProject.Domain.Factories
{
	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> instances
	/// </summary>
	public partial interface IDomainDataStoreFactory
	{
		IDataStore<Inventory.Entities.InventoryItemSummaryEntity> GetInventoryItemSummaryDataStore();
	}
}