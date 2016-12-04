using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.MongoDB.DataStores;
using MongoDB.Driver;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="MongoDbDataStore{TData}"/> class
	/// </summary>
	[TestClass]
	public class MongoDbDataStoreTests
	{
		[TestMethod]
		public void Save_ValidProjectionView_ProjectionViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper();
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettings(), correlationIdHelper);

			var connectionStringFactory = new TestMongoDataStoreConnectionStringFactory();
			TestMongoDataStoreConnectionStringFactory.DatabaseName = string.Format("Test-{0}", new Random().Next(0, 9999));

			var factory = new TestMongoDbDataStoreFactory(logger, connectionStringFactory);
			IMongoCollection<TestEvent> collection = factory.GetTestEventCollection();
			try
			{
				// Arrange
				var dataStore = new MongoDbDataStore<TestEvent>(logger, collection);

				var event1 = new TestEvent
				{
					Rsn = Guid.NewGuid(),
					Id = Guid.NewGuid(),
					CorrelationId = correlationIdHelper.GetCorrelationId(),
					Frameworks = new List<string> { "Test 1" },
					TimeStamp = DateTimeOffset.UtcNow
				};

				// Act
				dataStore.Add(event1);

				// Assert
				var timer = new Stopwatch();
				timer.Start();
				TestEvent view = dataStore.SingleOrDefault(e => e.Rsn == event1.Rsn);
				timer.Stop();
				Console.WriteLine("Load operation took {0}", timer.Elapsed);
				Assert.IsNotNull(view);
				Assert.AreEqual(event1.Id, view.Id);
			}
			finally
			{
				// Clean-up
				collection.Database.Client.DropDatabase(TestMongoDataStoreConnectionStringFactory.DatabaseName);
			}
		}
	}
}