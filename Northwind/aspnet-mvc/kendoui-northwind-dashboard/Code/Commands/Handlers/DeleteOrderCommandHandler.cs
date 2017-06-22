namespace KendoUI.Northwind.Dashboard.Code.Commands.Handlers
{
	using Cqrs.Domain;
	using Cqrs.Commands;

	/// <summary>
	/// Responds to any <see cref="DeleteOrderCommand"/>, loads the existing <see cref="Order"/> instance and passes the contents of the <see cref="DeleteOrderCommand"/> to it.
	/// </summary>
	public class DeleteOrderCommandHandler
		
		: ICommandHandler<string, DeleteOrderCommand>
	{
		protected IUnitOfWork<string> UnitOfWork { get; private set; }

		public DeleteOrderCommandHandler(IUnitOfWork<string> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of ICommandHandler<in DeleteOrderCommand>

		public void Handle(DeleteOrderCommand command)
		{
			// As this is a request to delete an existing order, load the existing order (which will automatically add it to the UnitOfWork)
			Order item = UnitOfWork.Get<Order>(command.Rsn);
			// Request the Order be deleted
			item.DeleteOrder();
			// Commit your changes and publish any events
			UnitOfWork.Commit();
		}

		#endregion
	}
}