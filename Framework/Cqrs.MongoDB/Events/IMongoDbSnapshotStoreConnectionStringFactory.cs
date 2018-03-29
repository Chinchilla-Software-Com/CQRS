#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A factory for getting connection strings and database names for Snapshot Store access.
	/// </summary>
	public interface IMongoDbSnapshotStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		string GetSnapshotStoreConnectionString();

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		string GetSnapshotStoreDatabaseName();
	}
}