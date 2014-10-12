using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	public interface IAzureDocumentDbEventStoreConnectionHelper
	{
		DocumentClient GetEventStoreConnectionClient();

		string GetEventStoreConnectionLogStreamName();
	}
}