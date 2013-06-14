using System;
using System.Collections.Generic;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel
{
	public interface IReadModelFacade
	{
		IEnumerable<InventoryItemListDto> GetInventoryItems();
		InventoryItemDetailsDto GetInventoryItemDetails(Guid id);
		IEnumerable<UserListDto> GetUsers();
		UserDetailsDto GetUserDetails(Guid id);
	}
}