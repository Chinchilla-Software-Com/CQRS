#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Cqrs.Entities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	/// <summary>
	/// A <see cref="TableStorageStore{TData,TCollectionItemData}"/> that uses Azure Storage for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="TableEntity"/> Azure Table Storage will contain.</typeparam>
	public class TableStorageDataStore<TData>
		: TableStorageStore<EntityTableEntity<TData>, TData>
		where TData : Entity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
		/// </summary>
		public TableStorageDataStore(ILogger logger, ITableStorageDataStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
			: base(logger)
		{
			GetContainerName = tableStorageDataStoreConnectionStringFactory.GetTableName<TData>;
			IsContainerPublic = () => false;

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialise(tableStorageDataStoreConnectionStringFactory);
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public override void Remove(TData data)
		{
			data.IsDeleted = true;
			Update(data);
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
		/// Gets a <see cref="TableOperation"/> that calls <see cref="TableOperation.Retrieve{TData}(string,string,System.Collections.Generic.List{string})"/>
		/// for updating.
		/// </summary>
		/// <param name="data">The data containing the <see cref="IEntity.Rsn"/> property populated.</param>
		protected override TableOperation GetUpdatableTableEntity(TData data)
		{
			return TableOperation.Retrieve<EntityTableEntity<TData>>(data.GetType().FullName, data.Rsn.ToString("N"));
		}

		/// <summary>
		/// Gets a <see cref="TableOperation"/> that calls <see cref="TableOperation.Retrieve{TData}(string,string,System.Collections.Generic.List{string})"/>
		/// for updating.
		/// </summary>
		/// <param name="data">The <see cref="EntityTableEntity{TEntity}"/> containing the <see cref="EntityTableEntity{TEntity}.Entity"/> containing the <see cref="IEntity.Rsn"/> property populated.</param>
		protected override TableOperation GetUpdatableTableEntity(EntityTableEntity<TData> data)
		{
			return GetUpdatableTableEntity(data.Entity);
		}

		#endregion
	}
}