using System.Collections.Generic;
using Cqrs.Repositories.Queries;
using Cqrs.Services;
using Northwind.Domain.Orders.Entities;
using Northwind.Domain.Orders.Repositories.Queries.Strategies;

namespace Northwind.Domain.Orders.Services
{
	public partial class OrderService 
	{
		partial void OnGetAllOrders(IServiceRequestWithData<Cqrs.Authentication.ISingleSignOnToken, OrderServiceGetAllOrdersParameters> serviceRequest, ref IServiceResponseWithResultData<IEnumerable<OrderEntity>> results)
		{
			// Define Query
			ICollectionResultQuery<OrderQueryStrategy, OrderEntity> query = QueryFactory.CreateNewCollectionResultQuery<OrderQueryStrategy, OrderEntity>();

			// Retrieve Data
			query = OrderRepository.Retrieve(query);
			IEnumerable<OrderEntity> orders = query.Result;

			results = new ServiceResponseWithResultData<IEnumerable<OrderEntity>>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = orders
			};
		}
	}
}
