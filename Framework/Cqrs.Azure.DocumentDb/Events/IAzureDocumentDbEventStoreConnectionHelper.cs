using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	public interface IAzureDocumentDbEventStoreConnectionHelper
	{
		DocumentClient GetEventStoreConnection();

		string GetEventStoreConnectionLogStreamName();
	}
}