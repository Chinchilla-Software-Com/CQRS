using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;
using Northwind.Domain.Orders.Repositories;

namespace Northwind.Domain.Orders.Events.Handlers
{
	public  partial class OrderUpdatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IOrderRepository OrderRepository { get; private set; }

		public OrderUpdatedEventHandler(IAutomapHelper automapHelper, IOrderRepository repository)
		{
			AutomapHelper = automapHelper;
			OrderRepository = repository;
		}

		partial void OnHandle(OrderUpdated @event)
		{
			Entities.OrderEntity entity = OrderRepository.Load(@event.Rsn);
			entity = AutomapHelper.Automap(@event, entity);

			OrderRepository.Update(entity);
		}
	}
}
