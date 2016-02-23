namespace MyCompany.MyProject.Domain.Inventory.Commands.Handlers
{
	public partial class CheckInItemsToInventoryCommandHandler
	{
		partial void OnHandle(CheckInItemsToInventoryCommand command)
		{
			InventoryItem item = UnitOfWork.Get<InventoryItem>(command.Rsn);
			item.CheckIn(command.Count);
			UnitOfWork.Commit();
		}
	}
}