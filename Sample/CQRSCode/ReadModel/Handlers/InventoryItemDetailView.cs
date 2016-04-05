using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.ReadModel.Handlers
{
	public class InventoryItemDetailView : IEventHandler<ISingleSignOnToken, InventoryItemCreated>,
											IEventHandler<ISingleSignOnToken, InventoryItemDeactivated>,
											IEventHandler<ISingleSignOnToken, InventoryItemRenamed>,
											IEventHandler<ISingleSignOnToken, ItemsRemovedFromInventory>,
											IEventHandler<ISingleSignOnToken, ItemsCheckedInToInventory>
	{
		public void Handle(InventoryItemCreated message)
		{
			var dto = new InventoryItemDetailsDto(message.Id, message.Name, 0, message.Version);
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					datastore.InventoryItemDetailsDtoStore.Add(dto);
			}
			else
				InMemoryDatabase.Details.Add(message.Id, dto);
		}

		public void Handle(InventoryItemRenamed message)
		{
			InventoryItemDetailsDto dto;
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
				{
					if (datastore.Details.TryGetValue(message.Id, out dto))
					{
						dto.Name = message.NewName;
						dto.Version = message.Version;
						datastore.InventoryItemDetailsDtoStore.Update(dto);
					}
				}
			}
			else
			{
				if (InMemoryDatabase.Details.TryGetValue(message.Id, out dto))
				{
					dto.Name = message.NewName;
					dto.Version = message.Version;
				}
			}
		}

		public void Handle(ItemsRemovedFromInventory message)
		{
			InventoryItemDetailsDto dto;
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
				{
					if (datastore.Details.TryGetValue(message.Id, out dto))
					{
						dto.CurrentCount -= message.Count;
						dto.Version = message.Version;
						datastore.InventoryItemDetailsDtoStore.Update(dto);
					}
				}
			}
			else
			{
				if (InMemoryDatabase.Details.TryGetValue(message.Id, out dto))
				{
					dto.CurrentCount -= message.Count;
					dto.Version = message.Version;
				}
			}
		}

		public void Handle(ItemsCheckedInToInventory message)
		{
			InventoryItemDetailsDto dto;
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
				{
					if (datastore.Details.TryGetValue(message.Id, out dto))
					{
						dto.CurrentCount += message.Count;
						dto.Version = message.Version;
						datastore.InventoryItemDetailsDtoStore.Update(dto);
					}
				}
			}
			else
			{
				if (InMemoryDatabase.Details.TryGetValue(message.Id, out dto))
				{
					dto.CurrentCount += message.Count;
					dto.Version = message.Version;
				}
			}
		}

		public void Handle(InventoryItemDeactivated message)
		{
			if (ReadModelFacade.UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					datastore.InventoryItemDetailsDtoStore.Remove(datastore.Details[message.Id]);
			}
			else
				InMemoryDatabase.Details.Remove(message.Id);
		}
	}
}
