namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public partial class RemoveItemsFromInventoryFailedDueNegativeCountEventHandler
	{
		partial void OnHandle(RemoveItemsFromInventoryFailedDueNegativeCount @event)
		{
			// Do nothing as this is an error
		}
	}
}