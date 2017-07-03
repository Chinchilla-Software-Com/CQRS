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

namespace Cqrs.Azure.BlobStorage.Events
{
	public class BlobStorageEventStoreConnectionStringFactory : IBlobStorageStoreConnectionStringFactory
	{
		public static string BlobStorageReadableEventStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.EventStore.Read.ConnectionStringName";

		public static string BlobStorageWritableEventStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.EventStore.Write.ConnectionStringName";

		public static string BlobStorageEventStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.EventStore.ConnectionStringName";

		public static string BlobStorageBaseContainerNameKey = "Cqrs.Azure.BlobStorage.EventStore.BaseContainerName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public BlobStorageEventStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public virtual IEnumerable<string> GetWritableConnectionStrings()
		{
			Logger.LogDebug("Getting blob storage writable connection strings", "BlobStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
			try
			{
				var collection = new List<string> ();

				string blobStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageWritableEventStoreConnectionStringKey);
				if (string.IsNullOrWhiteSpace(blobStorageWritableEventStoreConnectionString))
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' in the configuration file.", BlobStorageWritableEventStoreConnectionStringKey), "BlobStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
					blobStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageEventStoreConnectionStringKey);
				}

				int writeIndex = 1;
				while (!string.IsNullOrWhiteSpace(blobStorageWritableEventStoreConnectionString))
				{
					collection.Add(blobStorageWritableEventStoreConnectionString);

					blobStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(string.Format("{0}.{1}", BlobStorageWritableEventStoreConnectionStringKey, writeIndex));

					writeIndex++;
				}

				if (!collection.Any())
					throw new NullReferenceException();

				return collection;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", BlobStorageEventStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting blob storage writable connection string... Done", "BlobStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
			}
		}

		public virtual string GetReadableConnectionString()
		{
			Logger.LogDebug("Getting blob storage readable connection strings", "BlobStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
			try
			{
				string blobStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageReadableEventStoreConnectionStringKey);
				if (string.IsNullOrWhiteSpace(blobStorageWritableEventStoreConnectionString))
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' in the configuration file.", BlobStorageReadableEventStoreConnectionStringKey), "BlobStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
					blobStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(BlobStorageEventStoreConnectionStringKey);
				}

				if (string.IsNullOrWhiteSpace(blobStorageWritableEventStoreConnectionString))
					throw new NullReferenceException();

				return blobStorageWritableEventStoreConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", BlobStorageEventStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting blob storage readable connection string... Done", "BlobStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
			}
		}

		public virtual string GetBaseContainerName()
		{
			Logger.LogDebug("Getting blob storage base container name", "BlobStorageEventStoreConnectionStringFactory\\GetBaseContainerName");
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
				Logger.LogDebug("Getting blob storage base container name... Done", "BlobStorageEventStoreConnectionStringFactory\\GetBaseContainerName");
			}
		}
	}
}