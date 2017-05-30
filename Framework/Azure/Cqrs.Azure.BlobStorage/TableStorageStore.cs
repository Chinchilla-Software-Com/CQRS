#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.DataStores;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cqrs.Azure.BlobStorage
{
	public abstract class TableStorageStore<TData, TCollectionItemData>
		: StorageStore<TData, CloudTable>
		, IDataStore<TCollectionItemData>
		where TData : TableEntity, new()
	{
		public TableQuery<TData> Collection { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStorageStore{TData,TCollectionItemData}"/> class using the specified container.
		/// </summary>
		protected TableStorageStore(ILogger logger)
			: base(logger)
		{
		}

		#region Overrides of StorageStore<TData,CloudTable>

		protected override void Initialise(IStorageStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
		{
			base.Initialise(tableStorageDataStoreConnectionStringFactory);
			Collection = new TableQuery<TData>();
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

		protected virtual void AsyncSaveData<TSaveData, TResult>(TSaveData data, Func<TSaveData, CloudTable, TResult> function, Func<TSaveData, string> customFilenameFunction = null)
		{
			IList<Task> persistTasks = new List<Task>();
			foreach (Tuple<CloudStorageAccount, CloudTable> tuple in WritableCollection)
			{
				TSaveData taskData = data;
				CloudTable table = tuple.Item2;
				Task task = Task.Factory.StartNewSafely
				(
					() =>
					{
						AzureStorageRetryPolicy.ExecuteAction(() => function(taskData, table));
					}
				);
				persistTasks.Add(task);
			}

			bool anyFailed = Task.Factory.ContinueWhenAll(persistTasks.ToArray(), tasks =>
			{
				return tasks.Any(task => task.IsFaulted);
			}).Result;
			if (anyFailed)
				throw new AggregateException("Persisting data to table storage failed. Check the logs for more details.");
		}

		protected abstract ITableEntity CreateTableEntity(TCollectionItemData data);

		#region Implementation of IDataStore<TData>

		public override void Add(TData data)
		{
			Add(data);
		}

		public virtual void Add(ITableEntity data)
		{
			AsyncSaveData
			(
				data,
				(taskData, table) =>
				{
					try
					{
						TableOperation insertOperation = TableOperation.Insert(taskData);

						// Execute the insert operation.
						return table.Execute(insertOperation);
					}
					catch (StorageException exception)
					{
						var extendedErrorInformation = exception.RequestInformation.ExtendedErrorInformation;
						if (extendedErrorInformation != null)
							Logger.LogError("There was an issue persisting data to table storage.", exception: exception);
						else
							Logger.LogError(string.Format("There was an issue persisting data to table storage. Specifically {0} :: {1}", exception.RequestInformation.ExtendedErrorInformation.ErrorCode, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage), exception: exception);
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

		public override void Add(IEnumerable<TData> data)
		{
			Add(data);
		}

		public virtual void Add(IEnumerable<ITableEntity> data)
		{
			AsyncSaveData
			(
				data,
				(taskData, table) =>
				{
					try
					{
						// Create the batch operation.
						TableBatchOperation batchOperation = new TableBatchOperation();

						foreach (TData item in taskData)
							batchOperation.Insert(item);

						// Execute the insert operation.
						return table.ExecuteBatch(batchOperation);
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue persisting data to table storage.", exception: exception);
						throw;
					}
				}
			);
		}

		public override void Destroy(TData data)
		{
			AsyncSaveData
			(
				data,
				(taskData, table) =>
				{
					try
					{
						// Create a retrieve operation that takes a customer entity.
						TableOperation retrieveOperation = GetUpdatableTableEntity(taskData);

						// Execute the operation.
						TableResult retrievedResult = table.Execute(retrieveOperation);
						ITableEntity tableEntity = (ITableEntity)retrievedResult.Result;

						TableOperation deleteOperation = TableOperation.Delete(tableEntity);

						// Execute the delete operation.
						return table.Execute(deleteOperation);
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue deleting data from table storage.", exception: exception);
						throw;
					}
				}
			);
		}

		/*
		public virtual void Destroy(ITableEntity data)
		{
			AsyncSaveData
			(
				data,
				(taskData, table) =>
				{
					try
					{
						// Create a retrieve operation that takes a customer entity.
						TableOperation retrieveOperation = GetUpdatableTableEntity(taskData);

						// Execute the operation.
						TableResult retrievedResult = table.Execute(retrieveOperation);
						ITableEntity tableEntity = (ITableEntity)retrievedResult.Result;

						TableOperation deleteOperation = TableOperation.Delete(tableEntity);

						// Execute the delete operation.
						return table.Execute(deleteOperation);
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue deleting data from table storage.", exception: exception);
						throw;
					}
				}
			);
		}
		*/

		public virtual void Add(TCollectionItemData data)
		{
			// Create the TableOperation object that inserts the customer entity.
			Add(CreateTableEntity(data));
		}

		public virtual void Add(IEnumerable<TCollectionItemData> data)
		{
			// Create the TableOperation object that inserts the customer entity.
			Add(data.Select(tdata => (TData)CreateTableEntity(tdata)));
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public abstract void Remove(TCollectionItemData data);

		public virtual void Destroy(TCollectionItemData data)
		{
			Destroy((TData)CreateTableEntity(data));
		}

		public override void RemoveAll()
		{
			foreach (Tuple<CloudStorageAccount, CloudTable> tuple in WritableCollection)
				tuple.Item2.DeleteIfExists();
		}

		public virtual void Update(TCollectionItemData data)
		{
			Update((TData)CreateTableEntity(data));
		}

		public override void Update(TData data)
		{
			AsyncSaveData
			(
				data,
				(taskData, table) =>
				{
					try
					{
						// Create a retrieve operation that takes a customer entity.
						TableOperation retrieveOperation = GetUpdatableTableEntity(taskData);

						// Execute the operation.
						TableResult retrievedResult = table.Execute(retrieveOperation);
						ITableEntity tableEntity = ReplaceValues(retrievedResult, data);

						TableOperation updateOperation = TableOperation.Replace(tableEntity);

						// Execute the update operation.
						return table.Execute(updateOperation);
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

		protected abstract TableOperation GetUpdatableTableEntity(TCollectionItemData data);

		protected abstract TableOperation GetUpdatableTableEntity(TData data);

		/// <summary>
		/// Creates a <see cref="CloudTable"/> with the specified name <paramref name="sourceName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the <see cref="CloudTable"/> is</param>
		/// <param name="sourceName">The name of the <see cref="CloudTable"/>.</param>
		/// <param name="isPublic">Whether or not this <see cref="CloudTable"/> is publicly accessible.</param>
		protected override CloudTable CreateSource(CloudStorageAccount storageAccount, string sourceName, bool isPublic = true)
		{
			// Create the table client.
			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

			// Retrieve a reference to the table.
			CloudTable table = tableClient.GetTableReference(GetSafeSourceName(sourceName));

			// Create the table if it doesn't exist.
			try
			{
				table.CreateIfNotExists();
			}
			catch (StorageException exception)
			{
				var extendedErrorInformation = exception.RequestInformation.ExtendedErrorInformation;
				if (extendedErrorInformation != null)
					Logger.LogError("There was an issue creating the table.", exception: exception);
				else
					Logger.LogError(string.Format("There was an issue creating the table. Specifically {0} :: {1}", exception.RequestInformation.ExtendedErrorInformation.ErrorCode, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage), exception: exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError("There was an issue creating the table.", exception: exception);
				throw;
			}

			return table;
		}

		public virtual TData GetByKeyAndRow(Guid rsn)
		{
			// Create the table query.
			var rangeQuery = Collection.Where
			(
				TableQuery.CombineFilters
				(
					TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(typeof(TCollectionItemData).FullName)),
					TableOperators.And,
					TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(rsn.ToString("N")))
				)
			);

			return ReadableSource.ExecuteQuery(rangeQuery).Single();
		}

		public virtual IEnumerable<TData> GetByKey()
		{
			// Create the table query.
			var rangeQuery = Collection.Where
			(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, StorageStore<object, object>.GetSafeStorageKey(typeof(TCollectionItemData).FullName))
			);

			return ReadableSource.ExecuteQuery(rangeQuery);
		}

		protected virtual ITableEntity ReplaceValues(TableResult retrievedResult, TData data)
		{
			ITableEntity tableEntity = (ITableEntity)retrievedResult.Result;
			var eventTableEntity = tableEntity as IEventDataTableEntity<TData>;
			if (eventTableEntity != null)
				eventTableEntity.EventData = data;
			else
				((IEntityTableEntity<TCollectionItemData>)tableEntity).Entity = ((IEntityTableEntity<TCollectionItemData>)data).Entity;

			return tableEntity;
		}

		#region Overrides of StorageStore<TData,CloudTable>

		protected override string GetSafeSourceName(string sourceName)
		{
			return GetSafeSourceName(sourceName, false);
		}

		#endregion
	}
}