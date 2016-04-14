using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;
using Northwind.Domain.Orders.Repositories;

namespace Northwind.Domain.Orders.Events.Handlers
{
	public  partial class OrderDeletedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IOrderRepository OrderRepository { get; private set; }

		public OrderDeletedEventHandler(IAutomapHelper automapHelper, IOrderRepository repository)
		{
			AutomapHelper = automapHelper;
			OrderRepository = repository;
		}

		partial void OnHandle(OrderDeleted @event)
		{
			Entities.OrderEntity entity = OrderRepository.Load(@event.Rsn);
			entity.IsLogicallyDeleted = true;

			OrderRepository.Update(entity);
		}
	}
}
