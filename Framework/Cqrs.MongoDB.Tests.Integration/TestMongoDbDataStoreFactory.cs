using cdmdotnet.Logging;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.MongoDB.Factories;
using MongoDB.Driver;

namespace Cqrs.MongoDB.Tests.Integration
{
	public class TestMongoDbDataStoreFactory : MongoDbDataStoreFactory
	{
		public TestMongoDbDataStoreFactory(ILogger logger, IMongoDbDataStoreConnectionStringFactory mongoDbDataStoreConnectionStringFactory)
			: base(logger, mongoDbDataStoreConnectionStringFactory)
		{
		}

		public IMongoCollection<TestEvent> GetTestEventCollection()
		{
			return GetCollection<TestEvent>();
		}
	}
}