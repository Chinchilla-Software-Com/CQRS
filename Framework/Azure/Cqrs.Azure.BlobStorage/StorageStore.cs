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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.Azure.BlobStorage
{
	public abstract class StorageStore<TData, TSource>
		: IEnumerable<TData>
	{
		protected IList<Tuple<CloudStorageAccount, TSource>> WritableCollection { get; private set; }

		protected CloudStorageAccount ReadableStorageAccount { get; private set; }

		internal TSource ReadableSource { get; private set; }

		protected ILogger Logger { get; private set; }

		protected Func<string> GetContainerName { get; set; }

		protected Func<bool> IsContainerPublic { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StorageStore{TData,TSource}"/> class using the specified container.
		/// </summary>
		protected StorageStore(ILogger logger)
		{
			Logger = logger;
		}

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

		public abstract void Add(TData data);

		public virtual void Add(IEnumerable<TData> data)
		{
			foreach (TData dataItem in data)
				Add(dataItem);
		}

		public abstract void Destroy(TData data);

		public abstract void RemoveAll();

		public abstract void Update(TData data);

		#endregion

		/// <summary>
		/// Creates a <typeparam name="TSource" /> with the specified name <paramref name="sourceName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the container is</param>
		/// <param name="sourceName">The name of the source.</param>
		/// <param name="isPublic">Whether or not this source is publicly accessible.</param>
		protected abstract TSource CreateSource(CloudStorageAccount storageAccount, string sourceName, bool isPublic = true);

		protected virtual string GetSafeSourceName(string sourceName)
		{
			if (sourceName.Contains(":"))
				return sourceName;

			string safeContainerName = sourceName.Replace(@"\", @"/").Replace(@"/", @"-").ToLowerInvariant();
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
	}
}