#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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
using Chinchilla.Logging;
using Cqrs.Entities;
using Cqrs.Events;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Cqrs.Azure.BlobStorage
{
	/// <summary>
	/// A <see cref="IEnumerable{TData}"/> that uses Azure Storage for storage.
	/// </summary>
	public abstract class StorageStore<TData, TSource>
		: IEnumerable<TData>
	{
		/// <summary>
		/// Gets the collection of writeable <see cref="CloudStorageAccount"/>.
		/// </summary>
		protected IList<Tuple<CloudStorageAccount, TSource>> WritableCollection { get; private set; }

		/// <summary>
		/// Gets the readable <see cref="CloudStorageAccount"/>.
		/// </summary>
		protected CloudStorageAccount ReadableStorageAccount { get; private set; }

		/// <summary>
		/// Gets the readable <typeparamref name="TSource"/>.
		/// </summary>
		internal TSource ReadableSource { get; private set; }

		/// <summary>
		/// Gets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="Func{Tstring}"/> that returns the name of the container.
		/// </summary>
		protected Func<string> GetContainerName { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Func{Tstring}"/> that returns if the container is public or not.
		/// </summary>
		protected Func<bool> IsContainerPublic { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StorageStore{TData,TSource}"/> class using the specified container.
		/// </summary>
		protected StorageStore(ILogger logger)
		{
			Logger = logger;
		}

		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static StorageStore()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		/// <summary>
		/// Initialises the <see cref="StorageStore{TData,TSource}"/>.
		/// </summary>
		protected virtual void Initialise(IStorageStoreConnectionStringFactory storageDataStoreConnectionStringFactory)
		{
			WritableCollection = new List<Tuple<CloudStorageAccount, TSource>>();
			ReadableStorageAccount = CloudStorageAccount.Parse(storageDataStoreConnectionStringFactory.GetReadableConnectionString());
			ReadableSource = CreateSource(ReadableStorageAccount, GetContainerName(), IsContainerPublic());

			foreach (string writableConnectionString in storageDataStoreConnectionStringFactory.GetWritableConnectionStrings())
			{
				CloudStorageAccount storageAccount = CloudStorageAccount.Parse(writableConnectionString);
				TSource container = CreateSource(storageAccount, GetContainerName(), IsContainerPublic());

				WritableCollection.Add(new Tuple<CloudStorageAccount, TSource>(storageAccount, container));
			}
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public abstract IEnumerator<TData> GetEnumerator();

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
		public abstract Expression Expression { get; }

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public abstract Type ElementType { get; }

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public abstract IQueryProvider Provider { get; }

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			ReadableSource = default(TSource);
			ReadableStorageAccount = null;

			WritableCollection = null;
		}

		#endregion

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public abstract void Add(TData data);

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual void Add(IEnumerable<TData> data)
		{
			foreach (TData dataItem in data)
				Add(dataItem);
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public abstract void Destroy(TData data);

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public abstract void RemoveAll();

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public abstract void Update(TData data);

		#endregion

		/// <summary>
		/// Creates a <typeparamref name="TSource" /> with the specified name <paramref name="sourceName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the container is</param>
		/// <param name="sourceName">The name of the source.</param>
		/// <param name="isPublic">Whether or not this source is publicly accessible.</param>
		protected abstract TSource CreateSource(CloudStorageAccount storageAccount, string sourceName, bool isPublic = true);

		/// <summary>
		/// Gets the provided <paramref name="sourceName"/> in a safe to use in lower case format.
		/// </summary>
		/// <param name="sourceName">The name to make safe.</param>
		protected virtual string GetSafeSourceName(string sourceName)
		{
			return GetSafeSourceName(sourceName, true);
		}

		/// <summary>
		/// Gets the provided <paramref name="sourceName"/> in a safe to use in format.
		/// </summary>
		/// <param name="sourceName">The name to make safe.</param>
		/// <param name="lowerCaseName">Indicates if the generated name is forced into a lower case format.</param>
		protected virtual string GetSafeSourceName(string sourceName, bool lowerCaseName)
		{
			if (sourceName.Contains(":"))
				return sourceName;

			string safeContainerName = sourceName.Replace(@"\", @"/").Replace(@"/", @"-");
			if (lowerCaseName)
				safeContainerName = safeContainerName.ToLowerInvariant();
			if (safeContainerName.StartsWith("-"))
				safeContainerName = safeContainerName.Substring(1);
			if (safeContainerName.EndsWith("-"))
				safeContainerName = safeContainerName.Substring(0, safeContainerName.Length - 1);
			safeContainerName = safeContainerName.Replace(" ", "-");

			return safeContainerName;
		}

		/// <summary>
		/// Characters Disallowed in Key Fields
		/// 
		/// The following characters are not allowed in values for the PartitionKey and RowKey properties:
		/// 
		/// The forward slash (/) character
		/// The backslash (\) character
		/// The number sign (#) character
		/// The question mark (?) character
		/// Control characters from U+0000 to U+001F, including:
		/// The horizontal tab (\t) character
		/// The linefeed (\n) character
		/// The carriage return (\r) character
		/// Control characters from U+007F to U+009F
		/// </summary>
		/// <param name="sourceName"></param>
		/// <returns></returns>
		internal static string GetSafeStorageKey(string sourceName)
		{
			var sb = new StringBuilder();
			foreach (var c in sourceName
				.Where(c => c != '/'
							&& c != '\\'
							&& c != '#'
							&& c != '/'
							&& c != '?'
							&& !char.IsControl(c)))
				sb.Append(c);
			return sb.ToString();
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
		/// Deserialise the provided <paramref name="dataStream"/> from its <see cref="Stream"/> representation.
		/// </summary>
		/// <param name="dataStream">A <see cref="Stream"/> representation of an <typeparamref name="TData"/> to deserialise.</param>
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

		/// <summary>
		/// Deserialise the provided <paramref name="json"/> from its <see cref="string"/> representation.
		/// </summary>
		/// <param name="json">A <see cref="string"/> representation of an <typeparamref name="TData"/> to deserialise.</param>
		protected virtual TData Deserialise(string json)
		{
			using (var stringReader = new StringReader(json))
				using (var jsonTextReader = new JsonTextReader(stringReader))
					return GetSerialiser().Deserialize<TData>(jsonTextReader);
		}

		/// <summary>
		/// Serialise the provided <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The <typeparamref name="TData"/> being serialised.</param>
		/// <returns>A <see cref="Stream"/> representation of the provided <paramref name="data"/>.</returns>
		protected virtual Stream Serialise(TData data)
		{
			string dataContent = JsonConvert.SerializeObject(data, GetSerialisationSettings());

			byte[] byteArray = Encoding.UTF8.GetBytes(dataContent);
			var stream = new MemoryStream(byteArray);

			return stream;
		}

		/// <summary>
		/// Returns <see cref="DefaultSettings"/>
		/// </summary>
		/// <returns><see cref="DefaultSettings"/></returns>
		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}

		/// <summary>
		/// Creates a new <see cref="JsonSerializer"/> using the settings from <see cref="GetSerialisationSettings"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="JsonSerializer"/>.</returns>
		protected virtual JsonSerializer GetSerialiser()
		{
			JsonSerializerSettings settings = GetSerialisationSettings();
			return JsonSerializer.Create(settings);
		}
	}
}