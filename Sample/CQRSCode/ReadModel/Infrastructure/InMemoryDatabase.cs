using System;
using System.Collections.Generic;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel.Infrastructure
{
	public static class InMemoryDatabase 
	{
		public static Dictionary<Guid, InventoryItemDetailsDto> Details = new Dictionary<Guid, InventoryItemDetailsDto>();
		public static List<InventoryItemListDto> List = new List<InventoryItemListDto>();
		public static Dictionary<Guid, UserDetailsDto> UserDetails = new Dictionary<Guid, UserDetailsDto>();
		public static List<UserListDto> UserList = new List<UserListDto>();
	}
}