namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public partial class RenameInventoryItemFailedDueNoNewNameEventHandler
	{
		partial void OnHandle(RenameInventoryItemFailedDueNoNewName @event)
		{
			// Do nothing as this is an error
		}
	}
}