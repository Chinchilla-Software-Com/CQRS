namespace MyCompany.MyProject.Domain.Inventory.Commands.Handlers
{
	public partial class RemoveItemsFromInventoryCommandHandler
	{
		partial void OnHandle(RemoveItemsFromInventoryCommand command)
		{
			InventoryItem item = UnitOfWork.Get<InventoryItem>(command.Rsn);
			item.Remove(command.Count);
			UnitOfWork.Commit();
		}
	}
}
