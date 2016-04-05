using System;
using System.Collections.Generic;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Infrastructure;

namespace CQRSCode.ReadModel
{
	public class ReadModelFacade : IReadModelFacade
	{
		public static bool UseSqlDatabase { get; set; }

		public IEnumerable<InventoryItemListDto> GetInventoryItems()
		{
			IEnumerable<InventoryItemListDto> dataStore;

			if (UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					dataStore = datastore.List;
			}
			else
				dataStore = InMemoryDatabase.List;
			return dataStore;
		}

		public InventoryItemDetailsDto GetInventoryItemDetails(Guid id)
		{
			IDictionary<Guid, InventoryItemDetailsDto> dataStore;

			if (UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					dataStore = datastore.Details;
			}
			else
				dataStore = InMemoryDatabase.Details;
			return dataStore[id];
		}

		public IEnumerable<UserListDto> GetUsers()
		{
			IEnumerable<UserListDto> dataStore;

			if (UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					dataStore = datastore.UserList;
			}
			else
				dataStore = InMemoryDatabase.UserList;
			return dataStore;
		}

		public UserDetailsDto GetUserDetails(Guid id)
		{
			IDictionary<Guid, UserDetailsDto> dataStore;

			if (UseSqlDatabase)
			{
				using (var datastore = new SqlDatabase())
					dataStore = datastore.UserDetails;
			}
			else
				dataStore = InMemoryDatabase.UserDetails;
			return dataStore[id];
		}
	}
}