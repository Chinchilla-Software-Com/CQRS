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
using Cqrs.Events;

namespace Cqrs.Azure.BlobStorage.Events
{
	public class BlobStorageEventStoreConnectionStringFactory : IBlobStorageStoreConnectionStringFactory
	{
		public static string BlobStorageReadableEventStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.EventStore.Read.ConnectionStringName";

		public static string BlobStorageWritableEventStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.EventStore.Write.ConnectionStringName";

		public static string BlobStorageEventStoreConnectionStringKey = "Cqrs.Azure.BlobStorage.EventStore.ConnectionStringName";

		public static string BlobStorageBaseContainerNameKey = "Cqrs.Azure.BlobStorage.EventStore.BaseContainerName";

		public static string BlobStorageIsContainerPublicKey = "Cqrs.Azure.BlobStorage.EventStore.{0}.IsPublic";

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

				string blobStorageWritableEventStoreConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(BlobStorageWritableEventStoreConnectionStringKey)].ConnectionString;
				if (blobStorageWritableEventStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No connection string named '{0}' in the configuration file.", BlobStorageWritableEventStoreConnectionStringKey), "BlobStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
					blobStorageWritableEventStoreConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(BlobStorageEventStoreConnectionStringKey)].ConnectionString;
				}

				int writeIndex = 1;
				while (!string.IsNullOrWhiteSpace(blobStorageWritableEventStoreConnectionString))
				{
					collection.Add(blobStorageWritableEventStoreConnectionString);

					blobStorageWritableEventStoreConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(string.Format("{0}.{1}", BlobStorageWritableEventStoreConnectionStringKey, writeIndex))].ConnectionString;
					writeIndex++;
				}

				if (!collection.Any())
					throw new NullReferenceException();

				return collection;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No connection string named '{0}' in the configuration file.", BlobStorageEventStoreConnectionStringKey), exception);
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
				string blobStorageWritableEventStoreConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(BlobStorageReadableEventStoreConnectionStringKey)].ConnectionString;
				if (blobStorageWritableEventStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No connection string named '{0}' in the configuration file.", BlobStorageReadableEventStoreConnectionStringKey), "BlobStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
					blobStorageWritableEventStoreConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(BlobStorageEventStoreConnectionStringKey)].ConnectionString;
				}

				if (string.IsNullOrWhiteSpace(blobStorageWritableEventStoreConnectionString))
					throw new NullReferenceException();

				return blobStorageWritableEventStoreConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No connection string named '{0}' in the configuration file.", BlobStorageEventStoreConnectionStringKey), exception);
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