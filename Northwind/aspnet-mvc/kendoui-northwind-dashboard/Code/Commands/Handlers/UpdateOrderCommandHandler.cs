namespace KendoUI.Northwind.Dashboard.Code.Commands.Handlers
{
	using Cqrs.Domain;
	using Cqrs.Commands;

	/// <summary>
	/// Responds to any <see cref="UpdateOrderCommand"/>, loads the existing <see cref="Order"/> instance and passes the contents of the <see cref="UpdateOrderCommand"/> to it.
	/// </summary>
	public class UpdateOrderCommandHandler
		
		: ICommandHandler<string, UpdateOrderCommand>
	{
		protected IUnitOfWork<string> UnitOfWork { get; private set; }

		public UpdateOrderCommandHandler(IUnitOfWork<string> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of ICommandHandler<in UpdateOrderCommand>

		public void Handle(UpdateOrderCommand command)
		{
			// As this is a request to update an existing order, load the existing order (which will automatically add it to the UnitOfWork)
			Order item = UnitOfWork.Get<Order>(command.Rsn);
			// Request the Order be updated
			item.UpdateOrder(command.OrderId, command.CustomerId, command.EmployeeId, command.OrderDate, command.RequiredDate, command.ShippedDate, command.ShipViaId, command.Freight, command.ShipName, command.ShipAddress, command.ShipCity, command.ShipRegion, command.ShipPostalCode, command.ShipCountry);
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}