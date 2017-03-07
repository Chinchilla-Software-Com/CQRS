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

namespace Cqrs.Azure.BlobStorage.DataStores
{
	public class TableStorageDataStoreConnectionStringFactory : ITableStorageDataStoreConnectionStringFactory
	{
		public static string TableStorageReadableDataStoreConnectionStringKey = "Cqrs.Azure.TableStorage.DataStore.Read.ConnectionStringName";

		public static string TableStorageWritableDataStoreConnectionStringKey = "Cqrs.Azure.TableStorage.DataStore.Write.ConnectionStringName";

		public static string TableStorageDataStoreConnectionStringKey = "Cqrs.Azure.TableStorage.DataStore.ConnectionStringName";

		public static string TableStorageBaseContainerNameKey = "Cqrs.Azure.TableStorage.DataStore.BaseContainerName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public TableStorageDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public virtual IEnumerable<string> GetWritableConnectionStrings()
		{
			Logger.LogDebug("Getting table storage writable connection strings", "TableStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
			try
			{
				var collection = new List<string> ();

				string blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(TableStorageWritableDataStoreConnectionStringKey);
				if (blobStorageWritableDataStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageWritableDataStoreConnectionStringKey), "TableStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(TableStorageDataStoreConnectionStringKey);
				}

				int writeIndex = 1;
				while (!string.IsNullOrWhiteSpace(blobStorageWritableDataStoreConnectionString))
				{
					collection.Add(blobStorageWritableDataStoreConnectionString);

					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(string.Format("{0}.{1}", TableStorageWritableDataStoreConnectionStringKey, writeIndex));
					writeIndex++;
				}

				if (!collection.Any())
					throw new NullReferenceException();

				return collection;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageDataStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting table storage writable connection string... Done", "TableStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
			}
		}

		public virtual string GetReadableConnectionString()
		{
			Logger.LogDebug("Getting table storage readable connection strings", "TableStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
			try
			{
				string blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(TableStorageReadableDataStoreConnectionStringKey);
				if (blobStorageWritableDataStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageReadableDataStoreConnectionStringKey), "TableStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(TableStorageDataStoreConnectionStringKey);
				}

				if (string.IsNullOrWhiteSpace(blobStorageWritableDataStoreConnectionString))
					throw new NullReferenceException();

				return blobStorageWritableDataStoreConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageDataStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting table storage readable connection string... Done", "TableStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
			}
		}

		public string GetBaseContainerName()
		{
			Logger.LogDebug("Getting table storage base container name", "TableStorageDataStoreConnectionStringFactory\\GetContainerName");
			try
			{
				string result = ConfigurationManager.GetSetting(TableStorageBaseContainerNameKey);

				if (string.IsNullOrWhiteSpace(result))
					throw new NullReferenceException();

				return result;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application setting named '{0}' in the configuration file.", TableStorageBaseContainerNameKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting table storage base container name... Done", "TableStorageDataStoreConnectionStringFactory\\GetContainerName");
			}
		}

		public virtual string GetContainerName()
		{
			return GetBaseContainerName();
		}

		public virtual string GetTableName<TData>()
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