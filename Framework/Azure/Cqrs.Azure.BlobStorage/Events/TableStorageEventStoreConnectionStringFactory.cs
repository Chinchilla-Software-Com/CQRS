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
	public class TableStorageEventStoreConnectionStringFactory : ITableStorageStoreConnectionStringFactory
	{
		public static string TableStorageReadableEventStoreConnectionStringKey = "Cqrs.Azure.TableStorage.EventStore.Read.ConnectionStringName";

		public static string TableStorageWritableEventStoreConnectionStringKey = "Cqrs.Azure.TableStorage.EventStore.Write.ConnectionStringName";

		public static string TableStorageEventStoreConnectionStringKey = "Cqrs.Azure.TableStorage.EventStore.ConnectionStringName";

		public static string TableStorageBaseContainerNameKey = "Cqrs.Azure.TableStorage.EventStore.BaseContainerName";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		public TableStorageEventStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		public virtual IEnumerable<string> GetWritableConnectionStrings()
		{
			Logger.LogDebug("Getting table storage writable connection strings", "TableStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
			try
			{
				var collection = new List<string> ();

				string tableStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(TableStorageWritableEventStoreConnectionStringKey);
				if (string.IsNullOrWhiteSpace(tableStorageWritableEventStoreConnectionString))
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' in the configuration file.", TableStorageWritableEventStoreConnectionStringKey), "TableStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
					tableStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(TableStorageEventStoreConnectionStringKey);
				}

				int writeIndex = 1;
				while (!string.IsNullOrWhiteSpace(tableStorageWritableEventStoreConnectionString))
				{
					collection.Add(tableStorageWritableEventStoreConnectionString);

					tableStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(string.Format("{0}.{1}", TableStorageWritableEventStoreConnectionStringKey, writeIndex));

					writeIndex++;
				}

				if (!collection.Any())
					throw new NullReferenceException();

				return collection;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageEventStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting table storage writable connection string... Done", "TableStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
			}
		}

		public virtual string GetReadableConnectionString()
		{
			Logger.LogDebug("Getting table storage readable connection strings", "TableStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
			try
			{
				string tableStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(TableStorageReadableEventStoreConnectionStringKey);
				if (string.IsNullOrWhiteSpace(tableStorageWritableEventStoreConnectionString))
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' in the configuration file.", TableStorageReadableEventStoreConnectionStringKey), "TableStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
					tableStorageWritableEventStoreConnectionString = ConfigurationManager.GetSetting(TableStorageEventStoreConnectionStringKey);
				}

				if (string.IsNullOrWhiteSpace(tableStorageWritableEventStoreConnectionString))
					throw new NullReferenceException();

				return tableStorageWritableEventStoreConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application settings named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageEventStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting table storage readable connection string... Done", "TableStorageEventStoreConnectionStringFactory\\GetReadableConnectionStrings");
			}
		}

		public virtual string GetBaseContainerName()
		{
			Logger.LogDebug("Getting table storage base container name", "TableStorageEventStoreConnectionStringFactory\\GetBaseContainerName");
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
				Logger.LogDebug("Getting table storage base container name... Done", "TableStorageEventStoreConnectionStringFactory\\GetBaseContainerName");
			}
		}
	}
}