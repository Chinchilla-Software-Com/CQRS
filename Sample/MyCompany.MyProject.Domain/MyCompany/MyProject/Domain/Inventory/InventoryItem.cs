using MyCompany.MyProject.Domain.Inventory.Events;

namespace MyCompany.MyProject.Domain.Inventory
{
	public partial class InventoryItem
	{
		partial void OnChangeName(string newName)
		{
			if (string.IsNullOrEmpty(newName))
				ApplyChange(new RenameInventoryItemFailedDueNoNewName(Rsn));
			else
				ApplyChange(new InventoryItemRenamed(Rsn, newName));
		}

		partial void OnRemove(long count)
		{
			if (count <= 0)
				ApplyChange(new RemoveItemsFromInventoryFailedDueNegativeCount(Rsn, count));
			else
				ApplyChange(new ItemsRemovedFromInventory(Id, count));
		}

		partial void OnCheckIn(long count)
		{
			if (count <= 0)
				ApplyChange(new CheckInItemsToInventoryFailedDueNegativeCount(Rsn, count));
			else
				ApplyChange(new ItemsCheckedInToInventory(Id, count));
		}

		partial void OnDeactivate()
		{
			if (!Activated)
				ApplyChange(new DeactivateInventoryItemFailedDueAlreadyActivated(Rsn));
			else
				ApplyChange(new InventoryItemDeactivated(Id));
		}

		partial void OnApply(InventoryItemCreated @event)
		{
			Activated = true;
		}

	}
}