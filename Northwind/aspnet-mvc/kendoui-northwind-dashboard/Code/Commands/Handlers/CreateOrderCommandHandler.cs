namespace KendoUI.Northwind.Dashboard.Code.Commands.Handlers
{
	using Cqrs.Domain;
	using System;
	using Cqrs.Commands;
	using Cqrs.Configuration;
	using cdmdotnet.Logging;

	/// <summary>
	/// Responds to any <see cref="CreateOrderCommand"/>, creates new <see cref="Order"/> instance and passes the contents of the <see cref="CreateOrderCommand"/> to it.
	/// </summary>
	public class CreateOrderCommandHandler : ICommandHandler<string, CreateOrderCommand>
	{
		protected IUnitOfWork<string> UnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		public CreateOrderCommandHandler(IUnitOfWork<string> unitOfWork, IDependencyResolver dependencyResolver, ILogger logger)
		{
			UnitOfWork = unitOfWork;
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		#region Implementation of ICommandHandler<in CreateOrder>

		public void Handle(CreateOrderCommand command)
		{
			// As this is a request to create an order, create a new order and add it to the UnitOfWork
			Order item = new Order(DependencyResolver, Logger, command.Rsn == Guid.Empty ? Guid.NewGuid() : command.Rsn);
			UnitOfWork.Add(item);
			// Request the Order be created
			item.CreateOrder(command.OrderId, command.CustomerId, command.EmployeeId, command.OrderDate, command.RequiredDate, command.ShippedDate, command.ShipViaId, command.Freight, command.ShipName, command.ShipAddress, command.ShipCity, command.ShipRegion, command.ShipPostalCode, command.ShipCountry);
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}