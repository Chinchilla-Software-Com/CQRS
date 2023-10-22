#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Chinchilla.Logging;
using Cqrs.Entities;

namespace Cqrs.Azure.Storage.DataStores
{
	/// <summary>
	/// A <see cref="TableStorageStore{TData,TCollectionItemData}"/> that uses Azure Storage for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="ITableEntity"/> Azure Table Storage will contain.</typeparam>
	public class TableStorageDataStore<TData>
		: TableStorageStore<EntityTableEntity<TData>, TData>
		where TData : Entity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageDataStore{TData}"/> class using the specified container.
		/// </summary>
		public TableStorageDataStore(ILogger logger, ITableStorageDataStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
			: base(logger)
		{
			GetContainerName = tableStorageDataStoreConnectionStringFactory.GetTableName<TData>;
			IsContainerPublic = () => false;

#if NET472
			Initialise(tableStorageDataStoreConnectionStringFactory);
#else
			Task.Run(async () => {
				await InitialiseAsync(tableStorageDataStoreConnectionStringFactory);
			}).Wait();
#endif
		}

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public override
#if NET472
			void Remove
#else
			async Task RemoveAsync
#endif
				(TData data)
		{
			data.IsDeleted = true;
#if NET472
			Update
#else
			await UpdateAsync
#endif
				(data);
		}

		#endregion

		#region Overrides of TableStorageStore<TData>

		/// <summary>
		/// Creates a new instance of <see cref="EntityTableEntity{TData}"/> populating it with the provided <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The data to store.</param>
		protected override ITableEntity CreateTableEntity(TData data)
		{
			return new EntityTableEntity<TData>(data);
		}

		/// <summary>
		/// Gets a <see cref="Tuple{T1, T2}"/> that contains the PartitionKey and RowKey
		/// for updating.
		/// </summary>
		/// <param name="data">The data containing the <see cref="IEntity.Rsn"/> property populated.</param>
		protected override (string PartitionKey, string RowKey) GetUpdatableTableEntity(TData data)
		{
			return (data.GetType().FullName, data.Rsn.ToString("N"));
		}

		/// <summary>
		/// Gets a <see cref="Tuple{T1, T2}"/> that contains the PartitionKey and RowKey
		/// for updating.
		/// </summary>
		/// <param name="data">The <see cref="EntityTableEntity{TEntity}"/> containing the <see cref="EntityTableEntity{TEntity}.Entity"/> containing the <see cref="IEntity.Rsn"/> property populated.</param>
		protected override (string PartitionKey, string RowKey) GetUpdatableTableEntity(EntityTableEntity<TData> data)
		{
			return GetUpdatableTableEntity(data.Entity);
		}

		#endregion
	}
}