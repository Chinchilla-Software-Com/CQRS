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
using System.Text;
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
			Logger.LogDebug("Getting table storage writeable connection strings", "TableStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
			try
			{
				var collection = new List<string> ();

				string blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(TableStorageWritableDataStoreConnectionStringKey);
				if (blobStorageWritableDataStoreConnectionString == null)
				{
					Logger.LogDebug(string.Format("No application setting named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageWritableDataStoreConnectionStringKey), "TableStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
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
				throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageDataStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting table storage writeable connection string... Done", "TableStorageDataStoreConnectionStringFactory\\GetWritableConnectionStrings");
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
					Logger.LogDebug(string.Format("No application setting named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageReadableDataStoreConnectionStringKey), "TableStorageDataStoreConnectionStringFactory\\GetReadableConnectionStrings");
					blobStorageWritableDataStoreConnectionString = ConfigurationManager.GetSetting(TableStorageDataStoreConnectionStringKey);
				}

				if (string.IsNullOrWhiteSpace(blobStorageWritableDataStoreConnectionString))
					throw new NullReferenceException();

				return blobStorageWritableDataStoreConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the cloud storage connection string.", TableStorageDataStoreConnectionStringKey), exception);
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

		readonly char[] _alphanumericCharacters =
		{
			'0','1','2','3','4','5','6','7','8','9',
			'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
			'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
		};
		/// <remarks>https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/</remarks>
		public virtual string GetTableName<TData>()
		{
			Type type = typeof (TData);
			string fullTableName = type.AssemblyQualifiedName ?? "Entities";
			StringBuilder sb;

			string name = fullTableName;
			int index1 = name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase);
			int index2 = -1;
			if (index1 > -1)
				index2 = name.IndexOf(",", index1 + 1, StringComparison.InvariantCultureIgnoreCase);
			if (index2 > -1)
			{
				name = name.Substring(0, index2);
				string[] nameParts = name.Split(',');
				if (nameParts.Length == 2)
				{
					if (nameParts[0].StartsWith(nameParts[1].Trim()))
					{
						name = name.Substring(0, index1);
						sb = new StringBuilder();
						foreach (var c in name.Where(c => _alphanumericCharacters.Contains(c)))
							sb.Append(c);

						name = sb.ToString();
						if (name.Length > 36)
							name = name.Substring(name.Length - 36);

						return name;
					}
				}
			}
			else if (index1 > -1)
				name = name.Substring(0, index1);

			sb = new StringBuilder();
			foreach (var c in name.Where(c => _alphanumericCharacters.Contains(c)))
				sb.Append(c);

			name = sb.ToString();
			return name.Substring(0, 36);
		}
	}
}