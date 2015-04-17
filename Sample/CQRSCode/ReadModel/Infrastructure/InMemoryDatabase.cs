using System;
using System.Collections.Generic;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel.Infrastructure
{
	public static class InMemoryDatabase 
	{
		public static IDictionary<Guid, InventoryItemDetailsDto> Details
		{
			get
			{
				return new Cqrs.Repositories.InMemoryDatabase()
					.Get<InventoryItemDetailsDto>();
			}
		}

		public static IList<InventoryItemListDto> List
		{
			get
			{
				return new Cqrs.Repositories.InMemoryDatabase()
					.GetAll<InventoryItemListDto>();
			}
		}

		public static IDictionary<Guid, UserDetailsDto> UserDetails
		{
			get
			{
				return new Cqrs.Repositories.InMemoryDatabase()
					.Get<UserDetailsDto>();
			}
		}

		public static IList<UserListDto> UserList
		{
			get
			{
				return new Cqrs.Repositories.InMemoryDatabase()
					.GetAll<UserListDto>();
			}
		}
	}
}