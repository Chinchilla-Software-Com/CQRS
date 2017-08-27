#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	/// <summary>
	/// A factory for getting connections and database names for <see cref="IEventStore{TAuthenticationToken}"/> access.
	/// </summary>
	public interface IAzureDocumentDbEventStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets the current <see cref="DocumentClient"/>.
		/// </summary>
		DocumentClient GetEventStoreConnectionClient();

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		string GetEventStoreConnectionDatabaseName();

		/// <summary>
		/// Gets the current collection name.
		/// </summary>
		string GetEventStoreConnectionCollectionName();
	}
}