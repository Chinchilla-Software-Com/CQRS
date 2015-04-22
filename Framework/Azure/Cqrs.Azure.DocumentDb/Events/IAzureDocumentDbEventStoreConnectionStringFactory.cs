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