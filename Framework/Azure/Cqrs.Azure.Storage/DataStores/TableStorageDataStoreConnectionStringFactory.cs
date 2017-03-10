#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Configuration;
using cdmdotnet.Logging;

namespace Cqrs.Azure.Storage.DataStores
{
	public class TableStorageDataStoreConnectionStringFactory<TData> : BlobStorage.DataStores.TableStorageDataStoreConnectionStringFactory
	{
		public TableStorageDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
			: base(configurationManager, logger)
		{
		}

		#region Overrides of TableStorageDataStoreConnectionStringFactory

		public override string GetContainerName()
		{
			return GetTableName<TData>();
		}

		/// <remarks>https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/</remarks>
		public override string GetTableName<TData1>()
		{
			string tableName = base.GetTableName<TData>();
			if (tableName.Length > 34)
				tableName = tableName.Substring(tableName.Length - 34);
			return string.Format("{0}V2", tableName);
		}

		#endregion
	}
}