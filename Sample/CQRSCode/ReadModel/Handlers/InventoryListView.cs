using System.Linq;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.ReadModel.Handlers
{
	public class InventoryListView : IEventHandler<ISingleSignOnToken, InventoryItemCreated>,
										IEventHandler<ISingleSignOnToken, InventoryItemRenamed>,
										IEventHandler<ISingleSignOnToken, InventoryItemDeactivated>
	{
		public void Handle(InventoryItemCreated message)
		{
			InMemoryDatabase.List.Add(new InventoryItemListDto(message.Id, message.Name));
		}

		public void Handle(InventoryItemRenamed message)
		{
			InventoryItemListDto item = InMemoryDatabase.List.SingleOrDefault(x => x.Id == message.Id);
			item.Name = message.NewName;
		}

		public void Handle(InventoryItemDeactivated message)
		{
			InventoryItemListDto item = InMemoryDatabase.List.SingleOrDefault(x => x.Id == message.Id);
			InMemoryDatabase.List.Remove(item);
		}
	}
}