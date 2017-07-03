#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public interface IAzureDocumentDbDataStoreConnectionStringFactory
	{
		DocumentClient GetAzureDocumentDbConnectionClient();

		string GetAzureDocumentDbDatabaseName();

		string GetAzureDocumentDbCollectionName();

		bool UseOneCollectionPerDataStore();
	}
}