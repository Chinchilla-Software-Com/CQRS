namespace KendoUI.Northwind.Dashboard.Code.Repositories
{
	using Cqrs.Repositories;
	using Factories;
	using Queries.Strategies;

	public class OrderRepository : Repository<OrderQueryStrategy, OrderQueryStrategyBuilder, Entities.OrderEntity>, IOrderRepository
	{
		public OrderRepository(IDomainDataStoreFactory dataStoreFactory, OrderQueryStrategyBuilder orderQueryBuilder)
			: base(dataStoreFactory.GetOrderDataStore, orderQueryBuilder)
		{
		}
	}
}