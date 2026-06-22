#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.DataStores;

namespace Cqrs.MongoDB.Factories
{
	/// <summary>
	/// A factory for getting connection strings and database names for <see cref="IDataStore{TData}"/> access.
	/// </summary>
	public interface IMongoDbDataStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		string GetDataStoreConnectionString();

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		string GetDataStoreDatabaseName();
	}
}