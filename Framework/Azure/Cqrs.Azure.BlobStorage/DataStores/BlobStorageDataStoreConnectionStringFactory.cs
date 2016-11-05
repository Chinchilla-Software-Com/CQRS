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
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	public class BlobStorageDataStoreConnectionStringFactory : IBlobStorageDataStoreConnectionStringFactory
	{
		public static string BlobStorageReadableDataStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.DataStore.Read.ConnectionStringName";

		public static string BlobStorageWritableDataStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.DataStore.Write.ConnectionStringName";

		public static string BlobStorageDataStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.DataStore.ConnectionStringName";

		public static string BlobStorageBaseContainerNameKey = "Cqrs.Azure.BlobStorage.DataStore.BaseContainerName";

		public static string BlobStorageIsContainerPublicKey = "Cqrs.Azure.BlobStorage.DataStore.{0}.IsPublic";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public BlobStorageDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public virtual IEnumerable<string> GetWritableConnectionStrings()
		{
			Logger.LogDebug("Getting blob storage writable connection strings", "BlobStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
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
				Logger.LogDebug("Getting blob storage writable connection string... Done", "BlobStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
			}
		}

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

		public virtual string GetContainerName<TData>()
			where TData : Entity
		{
			Logger.LogDebug("Getting blob storage container name", "BlobStorageDataStoreConnectionStringFactory\\GetContainerName");

			string name = string.Format("{0}\\{1}", GetBaseContainerName(), GetEntityName<TData>());

			Logger.LogDebug("Getting blob storage container name... Done", "BlobStorageDataStoreConnectionStringFactory\\GetContainerName");

			return name;
		}

		public virtual bool IsContainerPublic<TData>()
			where TData : Entity
		{
			bool result;
			// We default to false to protect the data
			if (!ConfigurationManager.TryGetSetting(string.Format(BlobStorageIsContainerPublicKey, GetEntityName<TData>()), out result))
				result = false;

			return result;
		}

		public virtual string GetEntityName<TData>()
			where TData : Entity
		{
			Type type = typeof (TData);
			string name = type.AssemblyQualifiedName;
			int index1 = name.IndexOf(",");
			int index2 = -1;
			if (index1 > -1)
				index2 = name.IndexOf(",", index1 + 1);
			if (index2 > -1)
				return name.Substring(0, index2);
			if (index1 > -1)
				return name.Substring(0, index1);
			return name;
		}
	}
}