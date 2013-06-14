using CQRSCode.WriteModel.Commands;
using CQRSCode.WriteModel.Domain;
using Cqrs.Commands;
using Cqrs.Domain;

namespace CQRSCode.WriteModel.Handlers
{
	public class InventoryCommandHandlers : ICommandHandler<CreateInventoryItem>,
											ICommandHandler<DeactivateInventoryItem>,
											ICommandHandler<RemoveItemsFromInventory>,
											ICommandHandler<CheckInItemsToInventory>,
											ICommandHandler<RenameInventoryItem>
	{
		private readonly IUnitOfWork _unitOfWork;

		public InventoryCommandHandlers(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Handle(CreateInventoryItem message)
		{
			var item = new InventoryItem(message.Id, message.Name);
			_unitOfWork.Add(item);
			_unitOfWork.Commit();
		}

		public void Handle(DeactivateInventoryItem message)
		{
			var item = _unitOfWork.Get<InventoryItem>(message.Id, message.ExpectedVersion);
			item.Deactivate();
			_unitOfWork.Commit();
		}

		public void Handle(RemoveItemsFromInventory message)
		{
			var item = _unitOfWork.Get<InventoryItem>(message.Id, message.ExpectedVersion);
			item.Remove(message.Count);
			_unitOfWork.Commit();
		}

		public void Handle(CheckInItemsToInventory message)
		{
			var item = _unitOfWork.Get<InventoryItem>(message.Id, message.ExpectedVersion);
			item.CheckIn(message.Count);
			_unitOfWork.Commit();
		}

		public void Handle(RenameInventoryItem message)
		{
			var item = _unitOfWork.Get<InventoryItem>(message.Id, message.ExpectedVersion);
			item.ChangeName(message.NewName);
			_unitOfWork.Commit();
		}
	}
}
