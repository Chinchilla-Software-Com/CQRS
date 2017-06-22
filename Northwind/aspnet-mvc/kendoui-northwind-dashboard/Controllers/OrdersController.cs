namespace KendoUI.Northwind.Dashboard.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Code.Commands;
	using Code.Entities;
	using Code.Events;
	using Code.Repositories;
	using Code.Repositories.Queries.Strategies;
	using Cqrs.Commands;
	using Cqrs.Repositories.Queries;
	using Cqrs.Web.Mvc;
	using Kendo.Mvc.Extensions;
	using Kendo.Mvc.UI;
	using Models;

	public class OrdersController : Controller
	{
		public ActionResult Orders_Read([DataSourceRequest] DataSourceRequest request)
		{
			return Json(GetOrders().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
		}

		public ActionResult Orders_Create([DataSourceRequest]DataSourceRequest request, [ModelBinder(typeof(NullableGuidBinder))]OrderViewModel order)
		{
			if (ModelState.IsValid)
			{
				// Cast the view model into a create command, and publish the command, waiting for an OrderCreated event
				// The Telerik Northwind dashboard website is synchronous not asynchronous.
				var command = (CreateOrderCommand)order;
				OrderCreated orderCreatedEvent = CommandPublisher.PublishAndWait<CreateOrderCommand, OrderCreated>(command);
				// Refresh server defined values from data store.
				OrderViewModel newOrder = GetOrders(orderCreatedEvent.Rsn).Single();
				order.OrderID = newOrder.OrderID;
				order.Rsn = newOrder.Rsn;
			}
			return Json(new[] { order }.ToDataSourceResult(request, ModelState));
		}

		public ActionResult Orders_Update([DataSourceRequest]DataSourceRequest request, [ModelBinder(typeof(NullableGuidBinder))]OrderViewModel order)
		{
			if (ModelState.IsValid)
			{
				// Cast the view model into an update command, and publish the command, waiting for an OrderUpdated event
				// The Telerik Northwind dashboard website is synchronous not asynchronous.
				var command = (UpdateOrderCommand)order;
				CommandPublisher.PublishAndWait<UpdateOrderCommand, OrderUpdated>(command);
			}
			return Json(new[] { order }.ToDataSourceResult(request, ModelState));
		}

		public ActionResult Orders_Destroy([DataSourceRequest]DataSourceRequest request, [ModelBinder(typeof(NullableGuidBinder))]OrderViewModel order)
		{
			if (ModelState.IsValid)
			{
				// Cast the view model into an delete command, and publish the command, waiting for an OrderDeleted event
				// The Telerik Northwind dashboard website is synchronous not asynchronous.
				var command = (DeleteOrderCommand)order;
				CommandPublisher.PublishAndWait<DeleteOrderCommand, OrderDeleted>(command);
			}
			return Json(new[] { order }.ToDataSourceResult(request, ModelState));
		}

		public ActionResult Countries_Read()
		{

			var countries = GetOrders().GroupBy(o => o.ShipCountry).Select(group => new
			{
				Country = group.Key == null ? " Other" : group.Key
			}).OrderBy(c => c.Country).ToList();

			return Json(countries, JsonRequestBehavior.AllowGet);
		}

		private IQueryable<OrderViewModel> GetOrders(Guid? orderRsn = null, int? orderId = null)
		{
			// Define Query
			ICollectionResultQuery<OrderQueryStrategy, OrderEntity> query = QueryFactory.CreateNewCollectionResultQuery<OrderQueryStrategy, OrderEntity>();

			// We don't want entities we've marked as deleted.
			query.QueryStrategy.WithNoDeletedOrders();

			// If an orderRsn is provided, query on that
			if (orderRsn != null)
				query.QueryStrategy.WithRsn(orderRsn.Value);
			// If an orderId is provided, query on that
			if (orderId != null)
				query.QueryStrategy.WithOrderId(orderId.Value);

			// Retrieve Data
			query = OrderRepository.Retrieve(query);
			IEnumerable<OrderEntity> queryResults = query.Result;

			IQueryable<OrderViewModel> orders = queryResults
				.Select(x => (OrderViewModel)x)
				.AsQueryable();

			return orders;
		}

		protected IOrderRepository OrderRepository { get; private set; }

		protected IQueryFactory QueryFactory { get; private set; }

		protected IPublishAndWaitCommandPublisher<string> CommandPublisher { get; private set; }

		public OrdersController(IOrderRepository orderRepository, IQueryFactory queryFactory, IPublishAndWaitCommandPublisher<string> commandPublisher)
		{
			OrderRepository = orderRepository;
			QueryFactory = queryFactory;
			CommandPublisher = commandPublisher;
		}
	}
}