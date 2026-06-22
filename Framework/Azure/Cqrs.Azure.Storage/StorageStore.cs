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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Chinchilla.Logging;
using Cqrs.Entities;
using Cqrs.Events;
using Newtonsoft.Json;

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// A <see cref="IEnumerable{TData}"/> that uses Azure Storage for storage.
	/// </summary>
	public abstract class StorageStore<TData, TSource, TClient>
		: IEnumerable<TData>
	{
		/// <summary>
		/// Gets the collection of writeable <see cref="TableServiceClient"/>.
		/// </summary>
		protected IList<(TClient Client, TSource Table)> WritableCollection { get; private set; }

		/// <summary>
		/// Gets the readable <see cref="TableServiceClient"/>.
		/// </summary>
		protected TClient ReadableStorageAccount { get; private set; }

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
		/// Initializes a new instance of the <see cref="StorageStore{TData,TSource,TClient}"/> class using the specified container.
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
#if NETSTANDARD2_0
			RedirectAssembly("mscorlib", "System.Private.CoreLib");
#else
			RedirectAssembly("System.Private.CoreLib", "mscorlib");
#endif
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		/// <summary>
		/// Create a new <typeparamref name="TClient"/>
		/// </summary>
		protected abstract TClient CreateClient(string connectionString);

		/// <summary>
		/// Initialises the <see cref="StorageStore{TData,TSource,TClient}"/>.
		/// </summary>
		protected virtual
#if NET472
			void Initialise
#else
			async Task InitialiseAsync
#endif
				(IStorageStoreConnectionStringFactory storageDataStoreConnectionStringFactory)
		{
			WritableCollection = new List<(TClient, TSource)>();
			ReadableStorageAccount = CreateClient(storageDataStoreConnectionStringFactory.GetReadableConnectionString());
			ReadableSource =
#if NET472
				CreateSource
#else
				await CreateSourceAsync
#endif
					(ReadableStorageAccount, GetContainerName(), IsContainerPublic());

			foreach (string writableConnectionString in storageDataStoreConnectionStringFactory.GetWritableConnectionStrings())
			{
				var storageAccount = CreateClient(writableConnectionString);
				TSource container =
#if NET472
					CreateSource
#else
					await CreateSourceAsync
#endif
						(storageAccount, GetContainerName(), IsContainerPublic());

				WritableCollection.Add((storageAccount, container));
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
		public virtual void Dispose()
		{
			ReadableSource = default(TSource);
			ReadableStorageAccount = default(TClient);

			WritableCollection = null;
		}

		#endregion

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public abstract
#if NET472
			void Add
#else
			Task AddAsync
#endif
				(TData data);

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(IEnumerable<TData> data)
		{
			foreach (TData dataItem in data)
#if NET472
				Add
#else
				await AddAsync
#endif
					(dataItem);
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public abstract
#if NET472
			void Destroy
#else
			Task DestroyAsync
#endif
				(TData data);

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public abstract
#if NET472
			void RemoveAll
#else
			Task RemoveAllAsync
#endif
				();

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public abstract
#if NET472
			void Update
#else
			Task UpdateAsync
#endif
				(TData data);

		#endregion

		/// <summary>
		/// Creates a <typeparamref name="TSource" /> with the specified name <paramref name="sourceName"/> if it doesn't already exist.
		/// </summary>
		/// <param name="storageAccount">The storage account to create the container in</param>
		/// <param name="sourceName">The name of the source.</param>
		/// <param name="isPublic">Whether or not this source is publicly accessible.</param>
		protected abstract

#if NET472
					TSource CreateSource
#else
					Task<TSource> CreateSourceAsync
#endif
						(TClient storageAccount, string sourceName, bool isPublic = true);

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

		/// <summary>
		/// Redirect an assembly resolution, used heavily for polumorphic serialisation and deserialisation such as between .NET Core and the .NET Framework
		/// </summary>
		/// <param name="fromAssemblyShortName">The name of the assembly to redirect.</param>
		/// <param name="replacmentAssemblyShortName">The name of the replacement assembly.</param>
		/// <remarks>
		/// https://stackoverflow.com/questions/50190568/net-standard-4-7-1-could-not-load-system-private-corelib-during-serialization
		/// </remarks>
		public static void RedirectAssembly(string fromAssemblyShortName, string replacmentAssemblyShortName)
		{
			Console.WriteLine($"Adding custom resolver redirect rule form:{fromAssemblyShortName}, to:{replacmentAssemblyShortName}");
			ResolveEventHandler handler = null;
			handler = (sender, args) =>
			{
				// Use latest strong name & version when trying to load SDK assemblies
				var requestedAssembly = new AssemblyName(args.Name);
				Console.WriteLine($"RedirectAssembly >  requesting:{requestedAssembly}; replacment from:{fromAssemblyShortName}, to:{replacmentAssemblyShortName}");
				if (requestedAssembly.Name != fromAssemblyShortName)
					return null;

				try
				{
					Console.WriteLine($"Redirecting Assembly {fromAssemblyShortName} to: {replacmentAssemblyShortName}");
					var replacmentAssembly = Assembly.Load(replacmentAssemblyShortName);
					return replacmentAssembly;
				}
				catch (Exception e)
				{
					Console.WriteLine($"ERROR while trying to provide replacement Assembly {fromAssemblyShortName} to: {replacmentAssemblyShortName}");
					Console.WriteLine(e);
					return null;
				}
			};

			AppDomain.CurrentDomain.AssemblyResolve += handler;
		}
	}
}