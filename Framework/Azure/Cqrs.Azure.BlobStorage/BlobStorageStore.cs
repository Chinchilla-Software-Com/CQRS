#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Cqrs.Azure.BlobStorage
{
	/// <summary>
	/// A <see cref="IEnumerable{TData}"/> that uses Azure Blobl Storage for storage.
	/// </summary>
	public class BlobStorageStore<TData>
		: StorageStore<TData, CloudBlobContainer>
	{
		internal Func<TData, string> GenerateFileName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageStore{TData}"/> class using the specified container.
		/// </summary>
		public BlobStorageStore(ILogger logger)
			: base(logger)
		{
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public override IEnumerator<TData> GetEnumerator()
		{
			return OpenStreamsForReading()
				.Select(Deserialise)
				.GetEnumerator();
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
				return OpenStreamsForReading()
					.Select(Deserialise)
					.AsQueryable()
					.Expression;
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
				return OpenStreamsForReading()
					.Select(Deserialise)
					.AsQueryable()
					.ElementType;
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
			get { return OpenStreamsForReading()
					.Select(Deserialise)
					.AsQueryable()
					.Provider;
			}
		}

		#endregion

		/// <summary>
		/// Save the provided <paramref name="data"/> asynchronously.
		/// </summary>
		protected virtual void AsyncSaveData<TResult>(TData data, Func<TData, CloudBlockBlob, TResult> function, Func<TData, string> customFilenameFunction = null)
		{
			IList<Task> persistTasks = new List<Task>();
			foreach (Tuple<CloudStorageAccount, CloudBlobContainer> tuple in WritableCollection)
			{
				TData taskData = data;
				CloudBlobContainer container = tuple.Item2;
				Task task = Task.Factory.StartNewSafely
				(
					() =>
					{
						string fileName = string.Format("{0}.json", (customFilenameFunction ?? GenerateFileName)(taskData));
						CloudBlockBlob cloudBlockBlob = GetBlobReference(container, fileName);
						if (typeof(TResult) == typeof(Uri))
						{
							Uri uri = AzureStorageRetryPolicy.ExecuteAction(() => (Uri)(object)function(taskData, cloudBlockBlob));

							Logger.LogDebug(string.Format("The data entity '{0}' was persisted at uri '{1}'", fileName, uri));
						}
						else
							AzureStorageRetryPolicy.ExecuteAction(() => function(taskData, cloudBlockBlob));
					}
				);
				persistTasks.Add(task);
			}

			bool anyFailed = Task.Factory.ContinueWhenAll(persistTasks.ToArray(), tasks =>
			{
				return tasks.Any(task => task.IsFaulted);
			}).Result;
			if (anyFailed)
				throw new AggregateException("Persisting data to blob storage failed. Check the logs for more details.");
		}

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public override void Add(TData data)
		{
			AsyncSaveData
			(
				data,
				(taskData, cloudBlockBlob) =>
				{
					try
					{
						cloudBlockBlob.UploadFromStream(Serialise(taskData));
						cloudBlockBlob.Properties.ContentType = "application/json";
						cloudBlockBlob.SetProperties();
						return cloudBlockBlob.Uri;
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue persisting data to blob storage.", exception: exception);
						throw;
					}
				}
			);
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public override void Destroy(TData data)
		{
			AsyncSaveData
			(
				data,
				(taskData, cloudBlockBlob) =>
				{
					try
					{
						return cloudBlockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
					}
					catch (Exception exception)
					{
						Logger.LogError("There was an issue deleting data from blob storage.", exception: exception);
						throw;
					}
				}
			);
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public override void RemoveAll()
		{
			foreach (Tuple<CloudStorageAccount, CloudBlobContainer> tuple in WritableCollection)
				tuple.Item2.DeleteIfExists();
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public override void Update(TData data)
		{
			Add(data);
		}

		#endregion

		/// <summary>
		/// Creates a <see cref="CloudBlobContainer"/> with the specified name <paramref name="containerName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the <see cref="CloudBlobContainer"/> is</param>
		/// <param name="containerName">The name of the <see cref="CloudBlobContainer"/>.</param>
		/// <param name="isPublic">Whether or not this <see cref="CloudBlobContainer"/> is publicly accessible.</param>
		protected override CloudBlobContainer CreateSource(CloudStorageAccount storageAccount, string containerName, bool isPublic = true)
		{
			CloudBlobContainer container = null;
			AzureStorageRetryPolicy.ExecuteAction
			(
				() =>
				{
					CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
					container = blobClient.GetContainerReference(GetSafeSourceName(containerName));
					container.CreateIfNotExists();
					if (isPublic)
					{
						container.SetPermissions(new BlobContainerPermissions
						{
							PublicAccess = BlobContainerPublicAccessType.Blob
						});
					}
				}
			);

			return container;
		}

		/// <summary>
		/// Opens stream for reading from a block blob.
		/// </summary>
		protected virtual IEnumerable<Stream> OpenStreamsForReading(Func<CloudBlockBlob, bool> predicate = null, string blobPrefix = null, string folderName = null)
		{
			IEnumerable<IListBlobItem> blobs;
			if (!string.IsNullOrWhiteSpace(folderName))
			{
				CloudBlobDirectory container = ReadableSource.GetDirectoryReference(folderName);
				blobs = container.ListBlobs(true);
			}
			else
			{
				blobs = ReadableSource.ListBlobs(blobPrefix, true);
			}
			IEnumerable<CloudBlockBlob> query = blobs
				.Where(x => x is CloudBlockBlob)
				.Cast<CloudBlockBlob>();
			if (predicate != null)
				query = query.Where(predicate);
			return query.Select(x => x.OpenRead());
		}

		/// <summary>
		/// Gets a reference to a block blob in the container.
		/// </summary>
		/// <param name="container">The container to get the reference from</param>
		/// <param name="blobName">The name of the blob.</param>
		/// <returns>A reference to a block blob.</returns>
		protected virtual CloudBlockBlob GetBlobReference(CloudBlobContainer container, string blobName)
		{
			return container.GetBlockBlobReference(blobName);
		}

		/// <summary>
		/// Get <typeparamref name="TData"/> by its name.
		/// </summary>
		public virtual TData GetByName(string name)
		{
			return OpenStreamsForReading(blobPrefix: name.Replace("\\", "/"))
				.Select(Deserialise)
				.SingleOrDefault();
			/*
			return OpenStreamsForReading(x => x.Name == name)
				.Select(Deserialise)
				.SingleOrDefault();
			*/
		}

		/// <summary>
		/// Get all <typeparamref name="TData"/> items in the folder.
		/// </summary>
		public virtual IEnumerable<TData> GetByFolder(string folderName)
		{
			string folder = new Uri(string.Format(folderName.StartsWith("..\\") ? "http://l/2/{0}" : "http://l/{0}", folderName)).AbsolutePath.Substring(1);
			return OpenStreamsForReading(folderName: folder)
				.Select(Deserialise);
			/*
			return OpenStreamsForReading(x => x.Parent.StorageUri.PrimaryUri.AbsolutePath.StartsWith(new Uri(string.Format(folderName.StartsWith("..\\") ? "http://l/{0}/2/{1}" : "http://l/{0}/{1}", GetContainerName(), folderName)).AbsolutePath, StringComparison.InvariantCultureIgnoreCase))
				.Select(Deserialise);
			*/
		}
	}
}