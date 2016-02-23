namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public partial class DeactivateInventoryItemFailedDueAlreadyActivatedEventHandler
	{
		partial void OnHandle(DeactivateInventoryItemFailedDueAlreadyActivated @event)
		{
			// Do nothing as this is an error
		}
	}
}