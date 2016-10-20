#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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