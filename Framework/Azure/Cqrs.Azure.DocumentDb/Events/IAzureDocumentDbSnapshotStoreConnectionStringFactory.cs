#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Snapshots;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	/// <summary>
	/// A factory for getting connections and database names for <see cref="ISnapshotStore"/> access.
	/// </summary>
	public interface IAzureDocumentDbSnapshotStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets the current <see cref="DocumentClient"/>.
		/// </summary>
		DocumentClient GetSnapshotStoreConnectionClient();

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		string GetSnapshotStoreConnectionDatabaseName();

		/// <summary>
		/// Gets the current collection name.
		/// </summary>
		string GetSnapshotStoreConnectionCollectionName();
	}
}