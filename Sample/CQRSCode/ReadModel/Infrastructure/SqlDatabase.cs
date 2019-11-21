using System;
using System.Collections.Generic;
using System.Linq;
using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Cqrs.Configuration;
using Cqrs.DataStores;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel.Infrastructure
{
	public class SqlDatabase : IDisposable
	{
		public SqlDatabase()
		{
			InventoryItemDetailsDtoStore = new SqlDataStore<InventoryItemDetailsDto>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));
			InventoryItemListDtoStore = new SqlDataStore<InventoryItemListDto>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));
			UserDetailsDtoStore = new SqlDataStore<UserDetailsDto>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));
			UserListDtoStore = new SqlDataStore<UserListDto>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));
		}

		public SqlDataStore<InventoryItemDetailsDto> InventoryItemDetailsDtoStore { get; set; }

		public SqlDataStore<InventoryItemListDto> InventoryItemListDtoStore { get; set; }

		public SqlDataStore<UserDetailsDto> UserDetailsDtoStore { get; set; }

		public SqlDataStore<UserListDto> UserListDtoStore { get; set; }

		public IDictionary<Guid, InventoryItemDetailsDto> Details
		{
			get
			{
				return InventoryItemDetailsDtoStore.ToDictionary(x => x.Rsn);
			}
		}

		public IList<InventoryItemListDto> List
		{
			get
			{
				return InventoryItemListDtoStore.ToList();
			}
		}

		public IDictionary<Guid, UserDetailsDto> UserDetails
		{
			get
			{
				return UserDetailsDtoStore.ToDictionary(x => x.Rsn);
			}
		}

		public IList<UserListDto> UserList
		{
			get
			{
				return UserListDtoStore.ToList();
			}
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			InventoryItemDetailsDtoStore.Dispose();
			InventoryItemListDtoStore.Dispose();
			UserDetailsDtoStore.Dispose();
			UserListDtoStore.Dispose();
		}

		#endregion
	}
}