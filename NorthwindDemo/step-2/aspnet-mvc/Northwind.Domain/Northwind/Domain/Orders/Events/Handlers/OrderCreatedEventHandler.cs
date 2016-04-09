using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;
using Northwind.Domain.Orders.Repositories;

namespace Northwind.Domain.Orders.Events.Handlers
{
	public  partial class OrderCreatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IOrderRepository OrderRepository { get; private set; }

		public OrderCreatedEventHandler(IAutomapHelper automapHelper, IOrderRepository repository)
		{
			AutomapHelper = automapHelper;
			OrderRepository = repository;
		}

		partial void OnHandle(OrderCreated @event)
		{
			Entities.OrderEntity entity = AutomapHelper.Automap<OrderCreated, Entities.OrderEntity>(@event);

			OrderRepository.Create(entity);
		}
	}
}
