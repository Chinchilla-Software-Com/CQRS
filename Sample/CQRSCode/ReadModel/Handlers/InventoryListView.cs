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
			var dto = new InventoryItemListDto(message.Id, message.Name);
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					datastore.InventoryItemListDtoStore.Add(dto);
			}
			else
				InMemoryDatabase.List.Add(dto);
		}

		public void Handle(InventoryItemRenamed message)
		{
			InventoryItemListDto dto;
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
				{
					dto = datastore.List.Single(x => x.Id == message.Id);
					dto.Name = message.NewName;
					datastore.InventoryItemListDtoStore.Update(dto);
				}
			}
			else
			{
				dto = InMemoryDatabase.List.Single(x => x.Id == message.Id);
				dto.Name = message.NewName;
			}
		}

		public void Handle(InventoryItemDeactivated message)
		{
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					datastore.InventoryItemListDtoStore.Remove(datastore.List.Single(x => x.Id == message.Id));
			}
			else
				InMemoryDatabase.List.Remove(InMemoryDatabase.List.Single(x => x.Id == message.Id));
		}
	}
}