using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Events;
using Cqrs.MongoDB.Events;
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
	/// A series of tests on the <see cref="MongoDbEventStore{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class MongoDbEventStoreTests
	{
		[TestMethod]
		public void Save_ValidEvent_EventCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper();
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			try
			{
				// Arrange
				var connectionStringFactory = new TestMongoEventStoreConnectionStringFactory();
				TestMongoEventStoreConnectionStringFactory.DatabaseName = string.Format("Test-{0}", new Random().Next(0, 9999));

				var eventStore = new MongoDbEventStore<Guid>(new DefaultEventBuilder<Guid>(), new EventDeserialiser<Guid>(), logger, connectionStringFactory);

				var event1 = new TestEvent
				{
					Rsn = Guid.NewGuid(),
					Id = Guid.NewGuid(),
					CorrelationId = correlationIdHelper.GetCorrelationId(),
					Frameworks = new List<string> { "Test 1" },
					TimeStamp = DateTimeOffset.UtcNow
				};
				var event2 = new TestEvent
				{
					Rsn = Guid.NewGuid(),
					Id = Guid.NewGuid(),
					CorrelationId = correlationIdHelper.GetCorrelationId(),
					Frameworks = new List<string> { "Test 2" },
					TimeStamp = DateTimeOffset.UtcNow
				};

				// Act
				eventStore.Save<TestEvent>(event1);
				eventStore.Save<TestEvent>(event2);

				// Assert
				var timer = new Stopwatch();
				IList<IEvent<Guid>> events = eventStore.Get<TestEvent>(event1.Id).ToList();
				timer.Stop();
				Console.WriteLine("Load one operation took {0}", timer.Elapsed);
				Assert.AreEqual(1, events.Count);
				Assert.AreEqual(event1.Id, events.Single().Id);
				Assert.AreEqual(event1.Frameworks.Single(), events.Single().Frameworks.Single());

				timer.Restart();
				events = eventStore.Get<TestEvent>(event2.Id).ToList();
				timer.Stop();
				Console.WriteLine("Load one operation took {0}", timer.Elapsed);
				Assert.AreEqual(1, events.Count);
				Assert.AreEqual(event2.Id, events.Single().Id);
				Assert.AreEqual(event2.Frameworks.Single(), events.Single().Frameworks.Single());

				timer.Restart();
				IList<EventData> correlatedEvents = eventStore.Get(event1.CorrelationId).ToList();
				timer.Stop();
				Console.WriteLine("Load several correlated operation took {0}", timer.Elapsed);
				Assert.AreEqual(2, correlatedEvents.Count);
			}
			finally
			{
				// Clean-up
				TestMongoDataStoreConnectionStringFactory.DatabaseName = TestMongoEventStoreConnectionStringFactory.DatabaseName;
				var factory = new TestMongoDbDataStoreFactory(logger, new TestMongoDataStoreConnectionStringFactory());
				IMongoCollection<TestEvent> collection = factory.GetTestEventCollection();
				collection.Database.Client.DropDatabase(TestMongoDataStoreConnectionStringFactory.DatabaseName);
			}
		}
	}
}