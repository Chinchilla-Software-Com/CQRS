#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using Cqrs.DataStores;
using Cqrs.Entities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	public class TableStorageDataStore<TData>
		: TableStorageStore<TData>
		, IDataStore<TData>
		where TData : Entity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
		/// </summary>
		public TableStorageDataStore(ILogger logger, ITableStorageDataStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
			: base(logger)
		{
			GetContainerName = tableStorageDataStoreConnectionStringFactory.GetContainerName;

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialise(tableStorageDataStoreConnectionStringFactory);
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public virtual void Remove(TData data)
		{
			data.IsLogicallyDeleted = true;
			Update(data);
		}

		#endregion

		#region Overrides of TableStorageStore<TData>

		protected override TableEntity CreateTableEntity(TData data)
		{
			return new EntityTableEntity<TData>(data);
		}

		protected override TableOperation GetUpdatableTableEntity(TData data)
		{
			return TableOperation.Retrieve<EntityTableEntity<TData>>(data.GetType().FullName, data.Rsn.ToString("N"));
		}

		#endregion
	}
}