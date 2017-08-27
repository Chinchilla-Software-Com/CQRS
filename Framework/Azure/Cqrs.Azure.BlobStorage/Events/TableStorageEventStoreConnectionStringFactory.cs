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
using Cqrs.Events;

namespace Cqrs.Azure.BlobStorage.Events
{
	/// <summary>
	/// A factory for getting connection strings and container names for <see cref="IEventStore{TAuthenticationToken}"/> access.
	/// This factory supports reading and writing from separate storage accounts. Specifically you can have as many different storage accounts as you want to configure when writing.
	/// This allows for manual mirroring of data while reading from the fastest/closest location possible.
	/// </summary>
	public class TableStorageEventStoreConnectionStringFactory : ITableStorageStoreConnectionStringFactory
	{
		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the connection string of the readable storage account if using a separate storage account for reads and writes.
		/// </summary>
		public static string TableStorageReadableEventStoreConnectionStringKey = "Cqrs.Azure.TableStorage.EventStore.Read.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the connection string of the writeable storage account if using a separate storage account for reads and writes.
		/// This value gets appended with a ".1", ".2" etc allowing you to write to as many different locations as possible.
		/// </summary>
		public static string TableStorageWritableEventStoreConnectionStringKey = "Cqrs.Azure.TableStorage.EventStore.Write.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the connection string if using a single storage account for both reads and writes.
		/// </summary>
		public static string TableStorageEventStoreConnectionStringKey = "Cqrs.Azure.TableStorage.EventStore.ConnectionStringName";

		/// <summary>
		/// The name of the app setting in <see cref="IConfigurationManager"/> that will have the base name of the container used.
		/// </summary>
		public static string TableStorageBaseContainerNameKey = "Cqrs.Azure.TableStorage.EventStore.BaseContainerName";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="TableStorageEventStoreConnectionStringFactory"/>.
		/// </summary>
		public TableStorageEventStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
		}

		/// <summary>
		/// Gets all writeable connection strings. If using a single storage account, then <see cref="TableStorageEventStoreConnectionStringKey"/> will most likely be returned.
		/// If a value for <see cref="TableStorageWritableEventStoreConnectionStringKey"/> is found, it will append ".1", ".2" etc returning any additionally found connection string values in <see cref="ConfigurationManager"/>.
		/// </summary>
		public virtual IEnumerable<string> GetWritableConnectionStrings()
		{
			Logger.LogDebug("Getting table storage writeable connection strings", "TableStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
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
				Logger.LogDebug("Getting table storage writeable connection string... Done", "TableStorageEventStoreConnectionStringFactory\\GetWritableConnectionStrings");
			}
		}

		/// <summary>
		/// Gets the readable connection string. If using a single storage account, then <see cref="TableStorageEventStoreConnectionStringKey"/> will most likely be returned.
		/// If a value for <see cref="TableStorageReadableEventStoreConnectionStringKey"/> is found, that will be returned instead.
		/// </summary>
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

		/// <summary>
		/// Returns the name of the base contain to be used.
		/// This will be the value from <see cref="ConfigurationManager"/> keyed <see cref="TableStorageBaseContainerNameKey"/>.
		/// </summary>
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