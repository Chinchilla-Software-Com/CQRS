namespace KendoUI.Northwind.Dashboard.Code.Repositories
{
	using Cqrs.Repositories;
	using Queries.Strategies;

	public interface IOrderRepository : IRepository<OrderQueryStrategy, Entities.OrderEntity>
	{
	}
}