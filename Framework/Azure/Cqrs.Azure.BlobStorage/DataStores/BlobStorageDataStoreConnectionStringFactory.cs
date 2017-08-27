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
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	/// <summary>
	/// A factory for getting connection strings and container names for <see cref="IDataStore{TData}"/> access.
	/// This factory supports reading and writing from separate storage accounts. Specifically you can have as many different storage accounts as you want to configure when writing.
	/// This allows for manual mirroring of data while reading from the fastest/closest location possible.
	/// </summary>
	public class BlobStorageDataStoreConnectionStringFactory : IBlobStorageDataStoreConnectionStringFactory
	{
		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the connection string of the readable storage account if using a separate storage account for reads and writes.
		/// </summary>
		public static string BlobStorageReadableDataStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.DataStore.Read.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the connection string of the writeable storage account if using a separate storage account for reads and writes.
		/// This value gets appended with a ".1", ".2" etc allowing you to write to as many different locations as possible.
		/// </summary>
		public static string BlobStorageWritableDataStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.DataStore.Write.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the connection string if using a single storage account for both reads and writes.
		/// </summary>
		public static string BlobStorageDataStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.DataStore.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the base name of the container used.
		/// </summary>
		public static string BlobStorageBaseContainerNameKey = "Cqrs.Azure.BlobStorage.DataStore.BaseContainerName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will indicate if the container is public or not.
		/// </summary>
		public static string BlobStorageIsContainerPublicKey = "Cqrs.Azure.BlobStorage.DataStore.{0}.IsPublic";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="BlobStorageDataStoreConnectionStringFactory"/>.
		/// </summary>
		public BlobStorageDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		/// <summary>
		/// Gets all writeable connection strings. If using a single storage account, then <see cref="BlobStorageDataStoreConnectionStringKey"/> will most likely be returned.
		/// If a value for <see cref="BlobStorageWritableDataStoreConnectionStringKey"/> is found, it will append ".1", ".2" etc returning any additionally found connection string values in <see cref="ConfigurationManager"/>.
		/// </summary>
		public virtual IEnumerable<string> GetWritableConnectionStrings()
		{
			Logger.LogDebug("Getting blob storage writeable connection strings", "BlobStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
			try
			{
				var collection = new List<string> ();

				string blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageWritableDataStoreConnectionStringKey);
				if (blobStorageWritableDataStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", BlobStorageWritableDataStoreConnectionStringKey), "BlobStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageDataStoreConnectionStringKey);
				}

				int writeIndex = 1;
				while (!string.IsNullOrWhiteSpace(blobStorageWritableDataStoreConnectionString))
				{
					collection.Add(blobStorageWritableDataStoreConnectionString);

					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(string.Format("{0}.{1}", BlobStorageWritableDataStoreConnectionStringKey, writeIndex));
					writeIndex++;
				}

				if (!collection.Any())
					throw new NullReferenceException();

				return collection;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", BlobStorageDataStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting blob storage writeable connection string... Done", "BlobStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
			}
		}

		/// <summary>
		/// Gets the readable connection string. If using a single storage account, then <see cref="BlobStorageDataStoreConnectionStringKey"/> will most likely be returned.
		/// If a value for <see cref="BlobStorageReadableDataStoreConnectionStringKey"/> is found, that will be returned instead.
		/// </summary>
		public virtual string GetReadableConnectionString()
		{
			Logger.LogDebug("Getting blob storage readable connection strings", "BlobStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
			try
			{
				string blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageReadableDataStoreConnectionStringKey);
				if (blobStorageWritableDataStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", BlobStorageReadableDataStoreConnectionStringKey), "BlobStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageDataStoreConnectionStringKey);
				}

				if (string.IsNullOrWhiteSpace(blobStorageWritableDataStoreConnectionString))
					throw new NullReferenceException();

				return blobStorageWritableDataStoreConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", BlobStorageDataStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting blob storage readable connection string... Done", "BlobStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
			}
		}

		/// <summary>
		/// Returns the name of the base contain to be used.
		/// This will be the value from <see cref="ConfigurationManager"/> keyed <see cref="BlobStorageBaseContainerNameKey"/>.
		/// </summary>
		public virtual string GetBaseContainerName()
		{
			Logger.LogDebug("Getting blob storage base container name", "BlobStorageDataStoreConnectionStringFactory\\GetBaseContainerName");
			try
			{
				string result = ConfigurationManager.GetSetting(BlobStorageBaseContainerNameKey);

				if (string.IsNullOrWhiteSpace(result))
					throw new NullReferenceException();

				return result;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application setting named '{0}' in the configuration file.", BlobStorageBaseContainerNameKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting blob storage base container name... Done", "BlobStorageDataStoreConnectionStringFactory\\GetBaseContainerName");
			}
		}

		/// <summary>
		/// Returns <see cref="GetBaseContainerName"/> and <see cref="GetEntityName{TData}"/>.
		/// </summary>
		/// <returns><see cref="GetBaseContainerName"/> and <see cref="GetEntityName{TData}"/></returns>
		public virtual string GetContainerName<TData>()
			where TData : Entity
		{
			Logger.LogDebug("Getting blob storage container name", "BlobStorageDataStoreConnectionStringFactory\\GetContainerName");

			string name = string.Format("{0}\\{1}", GetBaseContainerName(), GetEntityName<TData>());

			Logger.LogDebug("Getting blob storage container name... Done", "BlobStorageDataStoreConnectionStringFactory\\GetContainerName");

			return name;
		}

		/// <summary>
		/// Get if the container is public or not.
		/// </summary>
		public virtual bool IsContainerPublic<TData>()
			where TData : Entity
		{
			bool result;
			// We default to false to protect the data
			if (!ConfigurationManager.TryGetSetting(string.Format(BlobStorageIsContainerPublicKey, GetEntityName<TData>()), out result))
				result = false;

			return result;
		}

		/// <summary>
		/// Gets the name of the entity that is safe for container use.
		/// </summary>
		public virtual string GetEntityName<TData>()
			where TData : Entity
		{
			Type type = typeof (TData);
			string name = type.AssemblyQualifiedName ?? string.Empty;
			int index1 = name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase);
			int index2 = -1;
			if (index1 > -1)
				index2 = name.IndexOf(",", index1 + 1, StringComparison.InvariantCultureIgnoreCase);
			if (index2 > -1)
				return name.Substring(0, index2);
			if (index1 > -1)
				return name.Substring(0, index1);
			return name;
		}
	}
}