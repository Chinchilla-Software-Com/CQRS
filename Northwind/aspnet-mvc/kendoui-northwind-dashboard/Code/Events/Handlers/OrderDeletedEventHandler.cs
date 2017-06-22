namespace KendoUI.Northwind.Dashboard.Code.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Entities;
	using Repositories;

	public class OrderDeletedEventHandler : IEventHandler<string, OrderDeleted>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IOrderRepository OrderRepository { get; private set; }

		public OrderDeletedEventHandler(IAutomapHelper automapHelper, IOrderRepository repository)
		{
			AutomapHelper = automapHelper;
			OrderRepository = repository;
		}

		#region Implementation of IEventHandler<in OrderDeleted>

		public void Handle(OrderDeleted @event)
		{
			// As this is an update of an existing order, load the existing order
			OrderEntity entity = OrderRepository.Load(@event.Rsn);
			// Mark the order as deleted, but leave it in the database, this way we can produce a report of deleted order that can be undeleted.
			entity.IsLogicallyDeleted = true;

			// As this is an update (logical delete) of an existing order, pass the updated entity to the Repository to be updated and persisted
			OrderRepository.Update(entity);
		}

		#endregion
	}
}