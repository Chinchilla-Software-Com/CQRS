#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Chinchilla.Logging;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> that uses Azure Storage for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="ITableEntity"/> Azure Table Storage will contain.</typeparam>
	/// <typeparam name="TCollectionItemData">The <see cref="Type"/> of <see cref="IEntity"/> the <see cref="IDataStore{TData}"/> will contain.</typeparam>
	public abstract class TableStorageStore<TData, TCollectionItemData>
		: StorageStore<TData, TableClient, TableServiceClient>
		, IDataStore<TCollectionItemData>
		where TData : class, ITableEntity, new()
	{
		/// <summary>
		/// Gets or set the <see cref="IQueryable{TData}"/>.
		/// </summary>
		public IQueryable<TData> Collection { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageStore{TData,TCollectionItemData}"/> class using the specified container.
		/// </summary>
		protected TableStorageStore(ILogger logger)
			: base(logger)
		{
		}

		#region Overrides of StorageStore<TData,TableClient>

		/// <summary>
		/// Create a new <see cref="TableServiceClient"/>
		/// </summary>
		protected override TableServiceClient CreateClient(string connectionString)
		{
			var tableClientOptions = new TableClientOptions();
			tableClientOptions.Retry.Mode = RetryMode.Exponential;
			tableClientOptions.Retry.Delay = TimeSpan.FromSeconds(10);
			tableClientOptions.Retry.MaxRetries = 6;
			return new TableServiceClient(connectionString, tableClientOptions);
		}

		/// <summary>
		/// Initialises the <see cref="StorageStore{TData,TSource,TClient}"/>.
		/// </summary>
		protected override
#if NET472
			void Initialise
#else
			async Task InitialiseAsync
#endif
				(IStorageStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
		{
#if NET472
			base.Initialise
#else
			await base.InitialiseAsync
#endif
				(tableStorageDataStoreConnectionStringFactory);
			Collection = ReadableSource.Query<TData>().AsQueryable();
		}

		#endregion

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<TCollectionItemData> IEnumerable<TCollectionItemData>.GetEnumerator()
		{
			throw new NotImplementedException("Use IEnumerable<TData>.GetEnumerator() directly");
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public override IEnumerator<TData> GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		#endregion

		#region Implementation of IQueryable

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </returns>
		public override Expression Expression
		{
			get
			{
				return Collection.Expression;
			}
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public override Type ElementType
		{
			get
			{
				return Collection.ElementType;
			}
		}

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public override IQueryProvider Provider
		{
			get
			{
				return Collection.Provider;
			}
		}

		#endregion

		/// <summary>
		/// Save the provided <paramref name="data"/> asynchronously.
		/// </summary>
		protected virtual
#if NET472
			void
#else
			async Task
#endif
				AsyncSaveData<TSaveData, TResult>(TSaveData data, Func<TSaveData, TableClient, TResult> function, Func<TSaveData, string> customFilenameFunction = null)
		{
			IList<Task> persistTasks = new List<Task>();
			foreach ((TableServiceClient Client, TableClient Table) tuple in WritableCollection)
			{
				TSaveData taskData = data;
				TableClient table = tuple.Table;
				Task task = SafeTask.RunSafely
				(
					() =>
					{
						TResult result = function(taskData, table);
						Task t = result as Task;
						if (t != null)
							t.Wait();
					}
				);
				persistTasks.Add(task);
			}

			bool anyFailed;
#if NET472
			anyFailed = Task.Factory.ContinueWhenAll(persistTasks.ToArray(), tasks =>
			{
				return tasks.Any(task => task.IsFaulted);
			}).Result;
#else
			Task t = Task.WhenAll(persistTasks);
			try
			{
				await t;
			}
			catch { }

			anyFailed = t.Status == TaskStatus.Faulted;
#endif
			if (anyFailed)
				throw new AggregateException("Persisting data to table storage failed. Check the logs for more details.");
		}

		/// <summary>
		/// Creates a new instance of <see cref="ITableEntity"/> populating it with the provided <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The data to store.</param>
		protected abstract ITableEntity CreateTableEntity(TCollectionItemData data);

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public override
#if NET472
			void Add
#else
			async Task AddAsync
#endif
			(TData data)
		{
#if NET472
			Add
#else
			await AddAsync
#endif
				(data);
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
			(ITableEntity data)
		{
#if NET472
#else
			await
#endif
				AsyncSaveData
			(
				data,
#if NET472
#else
				async
#endif
					(taskData, table) =>
				{
					try
					{
						var result =
#if NET472
							table.AddEntity
#else
							await table.AddEntityAsync
#endif
								(data);

						if (result.IsError)
							throw new RequestFailedException(result);

						return result;
					}
					catch (RequestFailedException exception)
					{
						Logger.LogError($"There was an issue persisting data to table storage. Specifically {exception.ErrorCode} :: {exception.Message}", exception: exception);
						throw;
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue persisting data to table storage.", exception: exception);
						throw;
					}
				}
			);
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public override
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(IEnumerable<TData> data)
		{
#if NET472
			Add
#else
			await AddAsync
#endif
				(data);
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(IEnumerable<ITableEntity> data)
		{
#if NET472
#else
			await
#endif
				AsyncSaveData
			(
				data,
#if NET472
#else
				async
#endif
					(taskData, table) =>
				{
					try
					{
						// Create a collection of TableTransactionActions and populate it with the actions for each entity.
						var batch = new List<TableTransactionAction>();
						foreach (ITableEntity entity in taskData)
							new TableTransactionAction(TableTransactionActionType.Add, entity);

						// Execute the transaction.
						return
#if NET472
							table.SubmitTransaction
#else
							await table.SubmitTransactionAsync
#endif
								(batch);
					}
					catch (RequestFailedException exception)
					{
						Logger.LogError($"There was an issue persisting data to table storage. Specifically {exception.ErrorCode} :: {exception.Message}", exception: exception);
						throw;
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue persisting data to table storage.", exception: exception);
						throw;
					}
				}
			);
		}
		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public override
#if NET472
			void Destroy
#else
			async Task DestroyAsync
#endif
			(TData data)
		{
#if NET472
#else
			await
#endif
				AsyncSaveData
			(
				data,
#if NET472
#else
				async
#endif
					(taskData, table) =>
				{
					try
					{
						(string PartitionKey, string RowKey) entityData = GetUpdatableTableEntity(taskData);
						var result =
#if NET472
							table.DeleteEntity
#else
							await table.DeleteEntityAsync
#endif
								(entityData.PartitionKey, entityData.RowKey);

						if (result.IsError)
							throw new RequestFailedException(result);

						return result;
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue deleting data from table storage.", exception: exception);
						throw;
					}
				}
			);
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(TCollectionItemData data)
		{
			// Create the TableOperation object that inserts the customer entity.
#if NET472
			Add
#else
			await AddAsync
#endif
				(CreateTableEntity(data));
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(IEnumerable<TCollectionItemData> data)
		{
			// Create the TableOperation object that inserts the customer entity.
#if NET472
			Add
#else
			await AddAsync
#endif
				(data.Select(tdata => (TData)CreateTableEntity(tdata)));
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public abstract
#if NET472
			void Remove
#else
			Task RemoveAsync
#endif
				(TCollectionItemData data);

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Destroy
#else
			async Task DestroyAsync
#endif
				(TCollectionItemData data)
		{
#if NET472
			Destroy
#else
			await DestroyAsync
#endif
			((TData)CreateTableEntity(data));
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public override
#if NET472
			void RemoveAll
#else
			async Task RemoveAllAsync
#endif
				()
		{
			foreach ((TableServiceClient Client, TableClient Table) tuple in WritableCollection)
#if NET472
				tuple.Table.Delete
#else
				await tuple.Table.DeleteAsync
#endif
					();
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Update
#else
			async Task UpdateAsync
#endif
				(TCollectionItemData data)
		{
#if NET472
			Update
#else
			await UpdateAsync
#endif
				((TData)CreateTableEntity(data));
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public override
#if NET472
			void Update
#else
			async Task UpdateAsync
#endif
				(TData data)
		{
#if NET472
#else
			await
#endif
				AsyncSaveData
			(
				data,
#if NET472
#else
				async
#endif
					(taskData, table) =>
				{
					try
					{
						var entityData = GetUpdatableTableEntity(taskData);
						Response<TData> retrievedResult = table.GetEntity<TData>(entityData.PartitionKey, entityData.RowKey);
#if NET472
						retrievedResult = table.GetEntity<TData>(entityData.PartitionKey, entityData.RowKey);
#else
						retrievedResult = await table.GetEntityAsync<TData>(entityData.PartitionKey, entityData.RowKey);
#endif

						var result =
#if NET472
							table.UpdateEntity
#else
							await table.UpdateEntityAsync
#endif
								(taskData, retrievedResult.Value.ETag);

						if (result.IsError)
							throw new RequestFailedException(result);

						return result;
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue updating data in table storage.", exception: exception);
						throw;
					}
				}
			);
		}

		#endregion

		/// <summary>
		/// Gets a <see cref="Tuple{T1, T2}"/> for updating.
		/// </summary>
		protected abstract (string PartitionKey, string RowKey) GetUpdatableTableEntity(TCollectionItemData data);

		/// <summary>
		/// Gets a <see cref="Tuple{T1, T2}"/> for updating.
		/// </summary>
		protected abstract (string PartitionKey, string RowKey) GetUpdatableTableEntity(TData data);

		/// <summary>
		/// Creates a <see cref="TableClient"/> with the specified name <paramref name="sourceName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the <see cref="TableClient"/> is</param>
		/// <param name="sourceName">The name of the <see cref="TableClient"/>.</param>
		/// <param name="isPublic">Whether or not this <see cref="TableClient"/> is publicly accessible.</param>
		protected override
#if NET472
			TableClient CreateSource
#else
			async Task<TableClient> CreateSourceAsync
#endif
				(TableServiceClient storageAccount, string sourceName, bool isPublic = true)
		{
			try
			{
				// Get a reference to the TableClient from the service client instance.
				TableClient tableClient = storageAccount.GetTableClient(sourceName);

				// Create the table if it doesn't exist.
#if NET472
				tableClient.CreateIfNotExists
#else
				await tableClient.CreateIfNotExistsAsync
#endif
					();

				return
#if NET472
					tableClient;
#else
					await Task.FromResult(tableClient);
#endif
			}
			catch (RequestFailedException exception)
			{
				Logger.LogError($"There was an issue creating the table. Specifically {exception.ErrorCode} :: {exception.Message}", exception: exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError("There was an issue creating the table.", exception: exception);
				throw;
			}
		}

		/// <summary>
		/// Retrieves the data from Azure Storage using <see cref="Collection"/>.
		/// </summary>
		public virtual TData GetByKeyAndRow(Guid rsn)
		{
			string partitionKey = StorageStore<object, object, object>.GetSafeStorageKey(typeof(TCollectionItemData).FullName);
			string rowKey = StorageStore<object, object, object>.GetSafeStorageKey(rsn.ToString("N"));
			try
			{
				var result = Collection
					.Where(e => e.PartitionKey == partitionKey && e.RowKey == rowKey)
					.Single();

				return result;
			}
			catch (InvalidOperationException exception)
			{
				if (exception.Message == "Sequence contains no elements")
					throw new InvalidOperationException($"No Table Entity found by '{partitionKey}\\{rowKey}'", exception);
				throw exception;
			}
		}

		/// <summary>
		/// Retrieves the data from Azure Storage using <see cref="Collection"/>.
		/// </summary>
		public virtual IEnumerable<TData> GetByKey()
		{
			string partitionKey = StorageStore<object, object, object>.GetSafeStorageKey(typeof(TCollectionItemData).FullName);
			return Collection
				.Where(e => e.PartitionKey == partitionKey)
				.ToList();
		}

		#region Overrides of StorageStore<TData,TableClient>

		/// <summary>
		/// Gets the provided <paramref name="sourceName"/> in a safe to use format.
		/// </summary>
		/// <param name="sourceName">The name to make safe.</param>
		protected override string GetSafeSourceName(string sourceName)
		{
			return GetSafeSourceName(sourceName, false);
		}

		#endregion
	}
}