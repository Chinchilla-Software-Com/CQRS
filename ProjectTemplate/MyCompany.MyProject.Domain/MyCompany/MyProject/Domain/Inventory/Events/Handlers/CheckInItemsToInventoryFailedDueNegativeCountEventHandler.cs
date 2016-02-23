namespace MyCompany.MyProject.Domain.Inventory.Events.Handlers
{
	public partial class CheckInItemsToInventoryFailedDueNegativeCountEventHandler
	{
		partial void OnHandle(CheckInItemsToInventoryFailedDueNegativeCount @event)
		{
			// Do nothing as this is an error
		}
	}
}