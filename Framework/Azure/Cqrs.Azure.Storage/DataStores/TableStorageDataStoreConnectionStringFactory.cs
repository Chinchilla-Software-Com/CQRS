#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Azure.BlobStorage.DataStores;
using Cqrs.DataStores;

namespace Cqrs.Azure.Storage.DataStores
{
	/// <summary>
	/// A factory for getting connection strings and container names for <see cref="IDataStore{TData}"/> access.
	/// This factory supports reading and writing from separate storage accounts. Specifically you can have as many different storage accounts as you want to configure when writing.
	/// This allows for manual mirroring of data while reading from the fastest/closest location possible.
	/// </summary>
	public class TableStorageDataStoreConnectionStringFactory<TData> : TableStorageDataStoreConnectionStringFactory
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="TableStorageDataStoreConnectionStringFactory{TData}"/>.
		/// </summary>
		public TableStorageDataStoreConnectionStringFactory(IConfigurationManager configurationManager, ILogger logger)
			: base(configurationManager, logger)
		{
		}

		#region Overrides of TableStorageDataStoreConnectionStringFactory

		/// <summary>
		/// Returns <see cref="GetTableName{TData1}"/>.
		/// </summary>
		/// <returns><see cref="GetTableName{TData1}"/></returns>
		public override string GetContainerName()
		{
			return GetTableName<TData>();
		}

		/// <summary>
		/// Generates the name of the table for <typeparamref name="TData1"/> that matches naming rules for Azure Storage.
		/// The value differs from <see cref="TableStorageDataStoreConnectionStringFactory.GetTableName{TData}"/> in that it appends "V2" to the end of the table name.
		/// </summary>
		/// <typeparam name="TData1">The <see cref="Type"/> of data to read or write.</typeparam>
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