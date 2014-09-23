using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb
{
	public interface IEventStoreConnectionHelper
	{
		DocumentClient GetEventStoreConnection();

		string GetEventStoreConnectionLogStreamName();
	}
}