namespace KendoUI.Northwind.Dashboard.Code.Factories
{
	using Cqrs.DataStores;
	using Entities;

	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> instances
	/// </summary>
	public interface IDomainDataStoreFactory
	{
		IDataStore<OrderEntity> GetOrderDataStore();
	}
}