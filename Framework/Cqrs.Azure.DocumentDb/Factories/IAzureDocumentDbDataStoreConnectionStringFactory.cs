#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public interface IAzureDocumentDbDataStoreConnectionStringFactory
	{
		DocumentClient GetAzureDocumentDbConnectionString();

		string GetAzureDocumentDbDatabaseName();
	}
}