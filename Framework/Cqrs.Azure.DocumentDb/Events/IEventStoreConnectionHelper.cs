using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	public interface IEventStoreConnectionHelper
	{
		DocumentClient GetEventStoreConnection();

		string GetEventStoreConnectionLogStreamName();
	}
}