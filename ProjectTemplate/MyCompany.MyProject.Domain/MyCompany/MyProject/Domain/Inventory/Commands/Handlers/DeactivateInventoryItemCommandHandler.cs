namespace MyCompany.MyProject.Domain.Inventory.Commands.Handlers
{
	public partial class DeactivateInventoryItemCommandHandler
	{
		partial void OnHandle(DeactivateInventoryItemCommand command)
		{
			InventoryItem item = UnitOfWork.Get<InventoryItem>(command.Rsn);
			item.Deactivate();
			UnitOfWork.Commit();
		}
	}
}