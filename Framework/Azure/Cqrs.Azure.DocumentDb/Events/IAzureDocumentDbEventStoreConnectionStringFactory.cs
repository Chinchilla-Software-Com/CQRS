#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	public interface IAzureDocumentDbEventStoreConnectionStringFactory
	{
		DocumentClient GetEventStoreConnectionClient();

		string GetEventStoreConnectionDatabaseName();

		string GetEventStoreConnectionCollectionName();
	}
}