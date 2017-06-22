namespace KendoUI.Northwind.Dashboard.Code.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Entities;
	using Repositories;

	public class OrderUpdatedEventHandler : IEventHandler<string, OrderUpdated>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IOrderRepository OrderRepository { get; private set; }

		public OrderUpdatedEventHandler(IAutomapHelper automapHelper, IOrderRepository repository)
		{
			AutomapHelper = automapHelper;
			OrderRepository = repository;
		}

		#region Implementation of IEventHandler<in OrderUpdated>

		public void Handle(OrderUpdated @event)
		{
			// As this is an update of an existing order, load the existing order
			OrderEntity entity = OrderRepository.Load(@event.Rsn);
			// As the entities match very closely, use the auto-mapper to save writing code.
			entity = AutomapHelper.Automap(@event, entity);

			// As this is an update of an existing order, pass the updated entity to the Repository to be updated and persisted
			OrderRepository.Update(entity);
		}

		#endregion
	}
}