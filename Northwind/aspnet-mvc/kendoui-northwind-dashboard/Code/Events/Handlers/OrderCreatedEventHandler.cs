namespace KendoUI.Northwind.Dashboard.Code.Events.Handlers
{
	using cdmdotnet.AutoMapper;
	using Cqrs.Events;
	using Entities;
	using Repositories;

	public class OrderCreatedEventHandler : IEventHandler<string, OrderCreated>
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IOrderRepository OrderRepository { get; private set; }

		public OrderCreatedEventHandler(IAutomapHelper automapHelper, IOrderRepository repository)
		{
			AutomapHelper = automapHelper;
			OrderRepository = repository;
		}

		#region Implementation of IEventHandler<in OrderCreated>

		public void Handle(OrderCreated @event)
		{
			// As the entities match very closely, use the auto-mapper to save writing code.
			OrderEntity entity = AutomapHelper.Automap<OrderCreated, OrderEntity>(@event);

			// As this is the creation of a new order, pass the entity to the Repository for creation and persisted
			OrderRepository.Create(entity);
		}

		#endregion
	}
}