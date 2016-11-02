#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using cdmdotnet.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.Azure.BlobStorage
{
	public class BlobStorageStore<TData>
		: IEnumerable<TData>
	{
		protected IList<Tuple<CloudStorageAccount, CloudBlobContainer>> WritableCollection { get; private set; }

		protected CloudStorageAccount ReadableStorageAccount { get; private set; }

		protected CloudBlobContainer ReadableContainer { get; private set; }

		protected ILogger Logger { get; private set; }

		protected Func<string> GetContainerName { get; set; }

		protected Func<bool> IsContainerPublic { get; set; }

		protected Func<TData, string> GenerateFileName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
		/// </summary>
		public BlobStorageStore(ILogger logger)
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Logger = logger;
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		protected virtual void Initialise(IBlobStorageStoreConnectionStringFactory blobStorageDataStoreConnectionStringFactory)
		{
			WritableCollection = new List<Tuple<CloudStorageAccount, CloudBlobContainer>>();
			ReadableStorageAccount = CloudStorageAccount.Parse(blobStorageDataStoreConnectionStringFactory.GetReadableConnectionString());
			ReadableContainer = CreateContainer(ReadableStorageAccount, GetContainerName(), IsContainerPublic());

			foreach (string writableConnectionString in blobStorageDataStoreConnectionStringFactory.GetWritableConnectionStrings())
			{
				CloudStorageAccount storageAccount = CloudStorageAccount.Parse(writableConnectionString);
				CloudBlobContainer container = CreateContainer(storageAccount, GetContainerName(), IsContainerPublic());

				WritableCollection.Add(new Tuple<CloudStorageAccount, CloudBlobContainer>(storageAccount, container));
			}
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TData> GetEnumerator()
		{
			return OpenStreamsForReading()
				.Select(Deserialise)
				.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of IQueryable

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </returns>
		public Expression Expression
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
		public Type ElementType
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
		public IQueryProvider Provider
		{
			get { return OpenStreamsForReading()
					.Select(Deserialise)
					.AsQueryable()
					.Provider;
			}
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			ReadableContainer = null;
			ReadableStorageAccount = null;

			WritableCollection = null;
		}

		#endregion

		#region Implementation of IDataStore<TData>

		public void Add(TData data)
		{
			foreach (Tuple<CloudStorageAccount, CloudBlobContainer> tuple in WritableCollection)
			{
				CloudBlockBlob cloudBlockBlob = GetBlobReference(tuple.Item2, GenerateFileName(data));
				Uri uri = AzureStorageRetryPolicy.ExecuteAction
				(
					() =>
					{
						cloudBlockBlob.UploadFromStream(Serialise(data));
						cloudBlockBlob.Properties.ContentType = "application/json";
						cloudBlockBlob.SetProperties();
						return cloudBlockBlob.Uri;
					}
				);

				Logger.LogDebug(string.Format("The data entity '{0}' was persisted at uri '{1}'", GenerateFileName(data), uri));
			}
		}

		public void Add(IEnumerable<TData> data)
		{
			foreach (TData dataItem in data)
				Add(dataItem);
		}

		public void Destroy(TData data)
		{
			foreach (Tuple<CloudStorageAccount, CloudBlobContainer> tuple in WritableCollection)
			{
				CloudBlockBlob cloudBlockBlob = GetBlobReference(tuple.Item2, GenerateFileName(data));
				AzureStorageRetryPolicy.ExecuteAction
				(
					() =>
					{
						cloudBlockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
					}
				);
			}
		}

		public void RemoveAll()
		{
			foreach (Tuple<CloudStorageAccount, CloudBlobContainer> tuple in WritableCollection)
				tuple.Item2.DeleteIfExists();
		}

		public void Update(TData data)
		{
			Add(data);
		}

		#endregion

		/// <summary>
		/// Creates a container with the specified name <paramref name="containerName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the container is</param>
		/// <param name="containerName">The name of the container.</param>
		/// <param name="isPublic">Whether or not this container is publicly accessible.</param>
		protected virtual CloudBlobContainer CreateContainer(CloudStorageAccount storageAccount, string containerName, bool isPublic = true)
		{
			CloudBlobContainer container = null;
			AzureStorageRetryPolicy.ExecuteAction
			(
				() =>
				{
					CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
					container = blobClient.GetContainerReference(GetSafeContainerName(containerName));
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

		protected virtual string GetSafeContainerName(string containerName)
		{
			if (containerName.Contains(":"))
				return containerName;

			string safeContainerName = containerName.Replace(@"\", @"/").Replace(@"/", @"-").ToLowerInvariant();
			if (safeContainerName.StartsWith("-"))
				safeContainerName = safeContainerName.Substring(1);
			if (safeContainerName.EndsWith("-"))
				safeContainerName = safeContainerName.Substring(0, safeContainerName.Length - 1);
			safeContainerName = safeContainerName.Replace(" ", "-");

			return safeContainerName;
		}

		/// <summary>
		/// Gets the default retry policy dedicated to handling transient conditions with Windows Azure Storage.
		/// </summary>
		protected virtual RetryPolicy AzureStorageRetryPolicy
		{
			get
			{
				RetryManager retryManager = EnterpriseLibraryContainer.Current.GetInstance<RetryManager>();
				RetryPolicy retryPolicy = retryManager.GetDefaultAzureStorageRetryPolicy();
				retryPolicy.Retrying += (sender, args) =>
				{
					var msg = string.Format("Retrying action - Count: {0}, Delay: {1}", args.CurrentRetryCount, args.Delay);
					Logger.LogWarning(msg, exception: args.LastException);
				};
				return retryPolicy;
			}
		}

		/// <summary>
		/// Opens stream for reading from a block blob.
		/// </summary>
		protected virtual IEnumerable<Stream> OpenStreamsForReading(Func<CloudBlockBlob, bool> predicate = null)
		{
			IEnumerable<IListBlobItem> blobs = ReadableContainer.ListBlobs(null, true);
			IEnumerable<CloudBlockBlob> query = blobs
				.Where(x => x is CloudBlockBlob)
				.Cast<CloudBlockBlob>();
			if (predicate != null)
				query = query.Where(predicate);
			return query.Select(x => x.OpenRead());
		}

		protected virtual TData Deserialise(Stream dataStream)
		{
			using (dataStream)
			{
				using (var reader = new StreamReader(dataStream))
				{
					string jsonContents = reader.ReadToEnd();
					TData obj = Deserialise(jsonContents);
					return obj;
				}
			}
		}

		protected virtual TData Deserialise(string json)
		{
			using (var stringReader = new StringReader(json))
				using (var jsonTextReader = new JsonTextReader(stringReader))
					return GetSerialiser().Deserialize<TData>(jsonTextReader);
		}

		protected virtual Stream Serialise(TData data)
		{
			string dataContent = JsonConvert.SerializeObject(data, GetSerialisationSettings());

			byte[] byteArray = Encoding.UTF8.GetBytes(dataContent);
			var stream = new MemoryStream(byteArray);

			return stream;
		}

		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				Converters = new List<JsonConverter> { new StringEnumConverter() },
			};
		}

		protected virtual JsonSerializer GetSerialiser()
		{
			JsonSerializerSettings settings = GetSerialisationSettings();
			return JsonSerializer.Create(settings);
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

		public virtual TData GetByName(string name)
		{
			return OpenStreamsForReading(x => x.Name == name)
				.Select(Deserialise)
				.SingleOrDefault();
		}

		public virtual IEnumerable<TData> GetByFolder(string folderName)
		{
			return OpenStreamsForReading(x => x.Parent.StorageUri.PrimaryUri.AbsolutePath.EndsWith(folderName))
				.Select(Deserialise);
		}
	}
}