namespace MyCompany.MyProject.Domain.Inventory.Commands.Handlers
{
	public partial class RenameInventoryItemCommandHandler
	{
		partial void OnHandle(RenameInventoryItemCommand command)
		{
			InventoryItem item = UnitOfWork.Get<InventoryItem>(command.Rsn);
			item.ChangeName(command.NewName);
			UnitOfWork.Commit();
		}
	}
}