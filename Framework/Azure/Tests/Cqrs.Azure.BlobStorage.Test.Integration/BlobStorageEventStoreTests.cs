using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Azure.BlobStorage.Events;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
using Cqrs.Events;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.BlobStorage.Test.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="BlobStorageEventStore{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class BlobStorageEventStoreTests
	{
		[TestMethod]
		public void Save_ValidEvent_EventCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper();
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var eventStore = new BlobStorageEventStore<Guid>(new DefaultEventBuilder<Guid>(), new EventDeserialiser<Guid>(), logger, new BlobStorageEventStoreConnectionStringFactory(new ConfigurationManager(), logger));

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
			IList<IEvent<Guid>> events = eventStore.Get<TestEvent>(event1.Id).ToList();
			Assert.AreEqual(1, events.Count);
			Assert.AreEqual(event1.Id, events.Single().Id);
			Assert.AreEqual(event1.Frameworks.Single(), events.Single().Frameworks.Single());

			events = eventStore.Get<TestEvent>(event2.Id).ToList();
			Assert.AreEqual(1, events.Count);
			Assert.AreEqual(event2.Id, events.Single().Id);
			Assert.AreEqual(event2.Frameworks.Single(), events.Single().Frameworks.Single());

			IList<EventData> correlatedEvents = eventStore.Get(event1.CorrelationId).ToList();
			Assert.AreEqual(2, correlatedEvents.Count);
		}
	}
}